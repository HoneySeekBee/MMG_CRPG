using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extentions;
using System.Threading.Tasks;
using DG.Tweening;
using static PartySetManager;
using Combat;
using Google.Protobuf.WellKnownTypes;
using System;
using Game.Data;
using Game.Core;
using Game.Combat;
using Game.Logging;

public class BattleMapManager : MonoBehaviour
{
    public static BattleMapManager Instance { get; private set; }

    // Network
    private CombatNetwork _combatNetwork;
    private CombatDirector _combatDirector;
    private CombatVfxPresenter _vfx;
    private CombatSnapshotApplier _snapshotApplier;
    private CombatActorFactory _actorFactory;

    private long _combatId;
    private StartCombatResponsePb _combatStart;

    // Map / Wave 
    private readonly Dictionary<long, GameObject> _actorObjects = new();
    private readonly Dictionary<long, CombatTeam> _actorTeams = new();
    private readonly Dictionary<long, Vector3> _playerSpawnPos = new();

    [SerializeField] private Dictionary<int, BatchSlot> monsterSlotByIndex = new();
    private readonly List<long> _enemyActorIds = new();
    private readonly Dictionary<long, int> _actorWaveIndex = new();

    [SerializeField] private GameObject MonsterBasePrefab;
    [SerializeField] private GameObject UserPartyObj;

    private StagePb stageData;

    // ���̵� 
    private bool _waitingReturnBeforeMapMove = false;
    private bool _isMapMoving = false;
    private int _waveIndexForMove = -1;
    private bool _combatTickEnabled = false; // ƽ�� ������ �Ǵ��� üũ 
    private bool _endReturnDone = false;
    private bool _stageCleared = false;

    private int _clientTick = 0;
    private bool _battleEnded = false;

    [Header("���� ����")]
    [SerializeField] private PartySlot[] monsterSlots;
    private string _logCursor = "";

    [SerializeField] private SkillFxDataList skillFxDb;
    private readonly Dictionary<long, int> _actorMasterIds = new();
    private readonly Dictionary<long, CombatActorView> _viewCache = new();

    [SerializeField] private CombatSpeedApplier combatSpeedApplier; // Unity ���� ���� ���ǵ� ����
    private bool _gotStageResult = false;
    private bool _finalWin = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _combatNetwork = new CombatNetwork();
    }
    private void Start()
    {
        skillFxDb.Build();
        EnsureFactory();
    }
    private void LateUpdate()
    {
        if (_isMapMoving) return;
        _snapshotApplier?.UpdateRender(Time.unscaledDeltaTime);
    }
    private void EnsureFactory()
    {
        if (_actorFactory != null) return;

        _actorFactory = new CombatActorFactory(
            parent: PartySetManager.Instance.transform,
            monsterBasePrefab: MonsterBasePrefab,
            getCharacterLevel: (characterId) =>
            {
                if (GameState.Instance.CurrentUser.UserCharactersDict.TryGetValue(characterId, out var c))
                    return c.Level;
                return 1;
            }
        );
    }

    public IEnumerator Set_BattleMap(Action action)
    {
        Set_MonsterSlot();
        EnsureFactory();
        _gotStageResult = false;
        _finalWin = false;

        _snapshotApplier?.Clear();

        // [1] ������ ���� ���� ��û
        int stageId = LobbyRootController.Instance._currentStage.Id;
        long battleId = LobbyRootController.Instance._currentBattleId;

        _combatStart = null;
        _battleEnded = false;
        _clientTick = 0;
        _endReturnDone = false;
        _stageCleared = false;

        GameLogger.Info($"���� ������ ��û {stageId} || {battleId}");

        yield return _combatNetwork.StartCombatAsync(stageId, battleId, res =>
        {
            if (!res.Ok)
            {
                GameLogger.Error($"[BattleMap] StartCombat ����: {res.Message}");
                return;
            }

            _combatStart = res.Data;
            _combatId = res.Data.CombatId;
        });

        if (_combatStart == null)
        {
            GameLogger.Error("[BattleMap] ���� ���� ���з� Set_BattleMap �ߴ�");
            yield break;
        }

        // [2] �� ����
        yield return Set_Map().AsCoroutine();

        // [3] ���� ������ ��� ���� ����
        _actorFactory.BuildFromSnapshot(
       _combatStart.Snapshot,
       actorObjects: _actorObjects,
       actorTeams: _actorTeams,
       actorWaveIndex: _actorWaveIndex,
       playerSpawnPos: _playerSpawnPos,
       actorMasterIds: _actorMasterIds,
       enemyActorIds: _enemyActorIds,
       onCreateSkillButton: (characterId, actorId, level) =>
       {
           BattleMapPopup.Instance.CreateSkillButton(characterId, actorId, level);
       }
   );

        // [4] view 캐시 구성
        _viewCache.Clear();
        foreach (var kv in _actorObjects)
        {
            var view = kv.Value.GetComponent<CombatActorView>();
            if (view != null) _viewCache[kv.Key] = view;
        }

        PartyMemeber();

        SetupCombatDirector(); // CombatDirect �ʱ�ȭ

        // [5] ���� ����/ƽ ���� ���� (������ ����ȭ) 
        StartCoroutine(TickLoop_CombatDirector()); // CombatDirector ��� ƽ ���� 
        StartCoroutine(BattleFlow());  // ����/ī�޶�/UI �帧

        action?.Invoke();
    }
    private void SetupCombatDirector()
    {
        _combatDirector = new CombatDirector(_combatNetwork);
        _combatDirector.Init(_combatId);

        _snapshotApplier = new CombatSnapshotApplier(_actorObjects, _actorTeams);
        _combatDirector.OnTickApplied += (snapshot, eventsThisTick) =>
        {
            _snapshotApplier.Apply(snapshot, eventsThisTick);
        };

        // CombatLog Event ó��
        _combatDirector.OnCombatEvent += HandleCombatEvent;

        // ���� ���� �ݹ�
        _combatDirector.OnBattleEnd += () =>
        {
            _battleEnded = true;
        };

        _vfx = new CombatVfxPresenter(skillFxDb, _actorObjects, _actorMasterIds, this.transform);
    }
    private IEnumerator TickLoop_CombatDirector()
    {  
        while (!_battleEnded)
        {
            // �� �̵� ���̸� ƽ �ȵ���
            if (!_combatTickEnabled)
            {
                yield return null;
                continue;
            }

            // CombatDirector�� Tick ó��
            yield return _combatDirector.Tick();
            yield return new WaitForSecondsRealtime(0.1f);
        } 
    }
    private void HandleCombatEvent(CombatLogEventPb ev)
    {
        GameLogger.Info($"[HandleCombatEvent] {ev.Type.ToString()}");
        _vfx?.HandleEvent(ev);
        switch (ev.Type)
        {
            case "spawn":
                HandleSpawnEvent(ev);
                break;

            case "wave_cleared":
                HandleWaveClearEvent(ev);
                break;

            case "stage_result":
                HandleStageResultEvent(ev);
                break;
        }
    }

    private void HandleStageResultEvent(CombatLogEventPb ev)
    {
        string result = "win";
        if (ev.Extra != null && ev.Extra.Fields.TryGetValue("result", out var v))
        {
            if (v.KindCase == Value.KindOneofCase.StringValue)
                result = v.StringValue;
        }

        _gotStageResult = true;
        _finalWin = (result == "win");
        _stageCleared = _finalWin;
        if (!_finalWin)
        {
            _battleEnded = true;
        }
        GameLogger.Info($"[BattleMap] stage_result = {result}");
    }
    private int GetBreakthrough(int characterId)
    {
        if (GameState.Instance.CurrentUser.UserCharactersDict.TryGetValue(characterId, out var c))
            return c.BreakThrough;
        return 0;
    }

    private void HandleWaveClearEvent(CombatLogEventPb ev)
    {
        int wave = -1;

        if (ev.Extra != null && ev.Extra.Fields.TryGetValue("wave", out var v))
        {
            if (v.KindCase == Value.KindOneofCase.StringValue)
                int.TryParse(v.StringValue, out wave);
            else if (v.KindCase == Value.KindOneofCase.NumberValue)
                wave = (int)v.NumberValue;
        }

        if (wave < 0)
        {
            GameLogger.Warn("[BattleMap] wave_cleared wave �Ľ� ����");
            return;
        }
        foreach (var p in GetAlivePlayerActors())
            p.transform.DOKill(complete: true);

        _waitingReturnBeforeMapMove = true;
        _waveIndexForMove = wave;

        GameLogger.Info($"[BattleMap] wave_cleared ���� wave={wave}");
    }
    private void Set_MonsterSlot()
    {
        monsterSlotByIndex.Clear();

        foreach (var item in monsterSlots)
        {
            monsterSlotByIndex[item.slotNum] = item.batchSlot;
        }
    }
    private async Task Set_Map()
    {
        stageData = LobbyRootController.Instance._currentStage;
        this.transform.position = Vector3.zero;
        foreach (var batch in stageData.Batches)
        {
            var UnitObj = await AddressableManager.Instance.LoadAsync<GameObject>(batch.UnitKey);
            var EnvObj = await AddressableManager.Instance.LoadAsync<GameObject>(batch.EnvKey);

            GameObject unit = Instantiate(UnitObj, this.transform);
            GameObject env = Instantiate(EnvObj, unit.transform);

            unit.transform.position = new Vector3(20 * (batch.BatchNum - 1) - 20, 0, 0);
        }
    }
    private IEnumerator Move_Map()
    {
        // [1] 1�ʵ��� �� �̵� 
        Vector3 goalPos = this.transform.position;
        goalPos.x -= 20;
        List<CombatActorView> allPlayer = GetAlivePlayerActors();
        foreach (var a in allPlayer)
        {
            a.PlayMove();
        }
        this.transform.DOMove(goalPos, 2f);
        yield return new WaitForSeconds(2);
        foreach (var a in allPlayer)
        {
            a.PlayIdle();
        }
    }


    private void PartyMemeber()
    {
        GameLogger.Info("���� �غ�");
    }
    #region Battle Flow
    private IEnumerator BattleFlow()
    {
        var popup = BattleMapPopup.Instance;
        yield return new WaitForSeconds(1);
        // [1] ���� ���� ����
        yield return popup.StartCoroutine(popup.ShowStart());

        GameLogger.Info("���� ĳ���͵� ���� �ִϸ��̼�");
        if (stageData.Batches.Count > 1)
        {
            _isMapMoving = true;
            yield return Move_Map();
            _isMapMoving = false;
        }
        _combatTickEnabled = true;

        while (!_battleEnded)
        {
            // �������� wave_cleared �̺�Ʈ�� �Դ��� üũ
            if (_waitingReturnBeforeMapMove)
            {
                _waitingReturnBeforeMapMove = false;

                GameLogger.Info($"[BattleFlow] ���̺� {_waveIndexForMove} Ŭ���� ���� �� �÷��̾� ���� ����");

                // [1] ���� ���̺� ����  �÷��̾� ������������ ����
                yield return StartCoroutine(ReturnPlayersToSpawn());

                // ������ ���̺����� Ȯ��
                bool isLastWave = IsLastWave(_waveIndexForMove);
                if (!isLastWave)
                {
                    AudioManager.Instance.PlaySFX("SFX_Enter");
                    // [2] ���� ���̺긦 ���� �� �̵�
                    GameLogger.Info("[BattleFlow] ���� ���̺�� �� �̵�");
                    _isMapMoving = true;
                    yield return Move_Map();
                    _isMapMoving = false;
                }
                else
                {
                    // [3] ������ ���̺� -> ���� ����
                    GameLogger.Info("[BattleFlow] ������ ���̺� ���� -> End ���� ����");

                    //yield return StartCoroutine(ReturnPlayersToSpawnEnd()); 
                    // ���� �Ϸ� �� ���� ����� �̵� 
                }
            }
            if (_gotStageResult)
            {
                if (_finalWin && !_endReturnDone)
                    yield return StartCoroutine(ReturnPlayersToSpawnEnd());

                _battleEnded = true;
                break;
            }
            // �������� ����
            yield return null;
        }

        GameLogger.Info("[BattleFlow] BattleFlow ���� ���� �� FinishCombat ��û");
        if (_gotStageResult && _finalWin)
        {
            // ���� End ���� �������� ����
            if (!_endReturnDone)
                yield return StartCoroutine(ReturnPlayersToSpawnEnd());
        }
        // [3] ������ FinishCombat ��û  
        FinishCombatResponsePb result = null;
        bool done = false;

        yield return _combatNetwork.FinishCombatAsync(
            _combatId,
            res =>
            {
                if (!res.Ok)
                {
                    GameLogger.Error("[BattleMap] FinishCombat ����: " + res.Message);
                    done = true;
                    return;
                }

                result = res.Data;
                done = true;
            });

        if (!done || result == null)
        {
            GameLogger.Error("[BattleMap] FinishCombat ��� ����");
            yield break;
        }
        if (_stageCleared)
            ApplyStageClearToClientProgress(result);

        popup.ShowResult(result);

        GameLogger.Info("[BattleMap] BattleFlow ��ü ����");
    }
    private void ApplyStageClearToClientProgress(FinishCombatResponsePb res)
    {
        var user = GameState.Instance.CurrentUser;
        var prog = user.StageProgress;

        prog.ApplyClear(
            stageId: res.StageId,
            stars: res.Stars
        );
    }
    private IEnumerator ReturnPlayersToSpawnEnd()
    {
        if (_endReturnDone) yield break;

        var players = GetAlivePlayerActors();
        if (players.Count == 0)
        {
            _endReturnDone = true;
            yield break;
        }

        foreach (var v in players)
            v.PlayMove();

        while (!AreAllPlayersAtSpawn())
            yield return null;

        foreach (var v in players)
            v.PlayVictory();

        _endReturnDone = true;
        GameLogger.Info("[BattleMap] ReturnPlayersToSpawnEnd �Ϸ�");
    }
    private IEnumerator ReturnPlayersToSpawn()
    {
        var players = GetAlivePlayerActors();

        if (players.Count == 0)
            yield break;
        foreach (var v in players)
            v.PlayMove();
        while (!AreAllPlayersAtSpawn())
            yield return null;

        foreach (var v in players) 
            v.FaceDirection(Vector3.right, smooth: false);
        foreach (var v in players)
            v.PlayIdle();

        GameLogger.Info("[BattleMap] ReturnPlayersToSpawn �Ϸ�");
    }
    private bool IsLastWave(int waveIndex)
    {
        return waveIndex >= stageData.Batches.Count - 1;
    }

    private void HandleSpawnEvent(CombatLogEventPb ev)
    {
        GameLogger.Info($"[SpawnEvent] raw actor={ev.Actor}, target={ev.Target}");

        if (long.TryParse(ev.Actor, out var actorId))
        {
            if (_actorObjects.TryGetValue(actorId, out var go))
            {
                if (!go.activeSelf)
                {
                    go.SetActive(true);
                    GameLogger.Info($"[BattleMap] HandleSpawnEvent: Actor {actorId} Ȱ��ȭ");
                }
            }
            else
            {
                GameLogger.Warn($"[BattleMap] HandleSpawnEvent: ActorId {actorId}�� �ش��ϴ� GameObject ����");
            }
        }
    }


    #endregion 
    private bool AreAllPlayersAtSpawn()
    {
        const float tolerance = 0.2f; // ���� ��� ����

        foreach (var kv in _actorObjects)
        {
            long actorId = kv.Key;
            GameObject go = kv.Value;

            // �÷��̾� ����
            if (!_actorTeams.TryGetValue(actorId, out var team) || team != CombatTeam.Player)
                continue;

            if (!_viewCache.TryGetValue(actorId, out var view))
                continue;

            if (view.Hp <= 0)
                continue;

            if (!_playerSpawnPos.TryGetValue(actorId, out var spawnPos))
                continue;

            float dist = Vector3.Distance(view.transform.position, spawnPos);
            if (dist > tolerance)
            {
                // �ϳ��� ���� �ָ� false
                return false;
            }
        }

        // ����ִ� �÷��̾���� ���� ���� ��ó�� ����
        return true;
    }
    private static int GetIntFromExtra(CombatLogEventPb ev, string key, int defaultValue)
    {
        if (ev.Extra == null) return defaultValue;
        if (!ev.Extra.Fields.TryGetValue(key, out var v)) return defaultValue;

        if (v.KindCase == Value.KindOneofCase.NumberValue) return (int)v.NumberValue;
        if (v.KindCase == Value.KindOneofCase.StringValue && int.TryParse(v.StringValue, out var i)) return i;

        return defaultValue;
    }

    private List<CombatActorView> GetAlivePlayerActors()
    {
        var result = new List<CombatActorView>();

        foreach (var kv in _actorObjects)
        {
            long actorId = kv.Key;
            GameObject go = kv.Value;

            if (!_actorTeams.TryGetValue(actorId, out var team))
                continue;

            if (team != CombatTeam.Player)
                continue;

            if (!_viewCache.TryGetValue(actorId, out var view))
                continue;

            if (view.Hp <= 0)
                continue;

            result.Add(view);
        }

        return result;
    }
    public void RequestSkill(long actorId, int skillId, Action<bool> onResult = null)
    {
        if (_battleEnded)
        {
            GameLogger.Warn("[BattleMap] RequestSkill - Battle Ended");
            onResult?.Invoke(false);
            return;
        }

        GameLogger.Info($"[RequestSkill] {actorId} : {skillId}");
        StartCoroutine(RequestSkillRoutine(actorId, skillId, onResult));
    }
    private IEnumerator RequestSkillRoutine(long actorId, int skillId, Action<bool> onResult)
    {
        ApiResult<Empty> response = default;

        yield return _combatNetwork.UseSkillAsync(
            _combatId,
            actorId,
            skillId,
            null,
            1,
            res => response = res
        );
        if (!response.Ok)
        {
            GameLogger.Error("[BattleMap] RequestSkillRoutine: " + response.Message);
            onResult?.Invoke(false);
            yield break;
        }
         
        onResult?.Invoke(true);
    }
    private void FaceForwardAllPlayers(bool smooth = false)
    {
        var players = GetAlivePlayerActors();
        foreach (var v in players)
        {
            v.FaceDirection(Vector3.right, smooth);
        }
    }

    public void OnClickSpeedButton()
    {
        StartCoroutine(
            _combatNetwork.ToggleSpeedAsync(
                _combatId,
                res =>
                {
                    if (res.Ok)
                    {
                        BattleMapPopup.Instance.SpeedBtn.UpdateSpeedUI(res.Data.Speed);
                        CombatTime.SetSpeed(res.Data.Speed);
                        combatSpeedApplier.RefreshAndApply(CombatTime.TimeScale);
                    }
                }
            )
        );
    }
}
