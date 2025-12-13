using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extentions;
using System.Threading.Tasks;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using DG.Tweening;
using System.CodeDom.Compiler;
using System.Linq;
using static PartySetManager;
using Combat;
using UnityEngine.TextCore.Text;
using WebServer.Protos.Monsters;
using Unity.VisualScripting;
using Google.Protobuf.WellKnownTypes;
using System;
using Game.Managers;
using Game.Data;
using Game.Core;
using PixPlays.ElementalVFX;
using Game.Combat;

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

    // 맵이동 
    private bool _waitingReturnBeforeMapMove = false;
    private bool _isMapMoving = false;
    private int _waveIndexForMove = -1;
    private bool _combatTickEnabled = false; // 틱을 돌려도 되는지 체크 
    private bool _endReturnDone = false;
    private bool _stageCleared = false;

    private int _clientTick = 0;
    private bool _battleEnded = false;

    [Header("몬스터 관련")]
    [SerializeField] private PartySlot[] monsterSlots;
    private string _logCursor = "";

    [SerializeField] private SkillFxDataList skillFxDb;
    private readonly Dictionary<long, int> _actorMasterIds = new();

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

        _snapshotApplier?.Clear();

        // [1] 서버에 전투 시작 요청
        int stageId = LobbyRootController.Instance._currentStage.Id;
        long battleId = LobbyRootController.Instance._currentBattleId;

        _combatStart = null;
        _battleEnded = false;
        _clientTick = 0;
        _endReturnDone = false;
        _stageCleared = false;

        Debug.Log($"전투 데이터 요청 {stageId} || {battleId}");

        yield return _combatNetwork.StartCombatAsync(stageId, battleId, res =>
        {
            if (!res.Ok)
            {
                Debug.LogError($"[BattleMap] StartCombat 실패: {res.Message}");
                return;
            }

            _combatStart = res.Data;
            _combatId = res.Data.CombatId;
        });

        if (_combatStart == null)
        {
            Debug.LogError("[BattleMap] 전투 시작 실패로 Set_BattleMap 중단");
            yield break;
        }

        // [2] 맵 생성
        yield return Set_Map().AsCoroutine();

        // [3] 서버 스냅샷 기반 유닛 생성
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

        // [4] UI 연계
        PartyMemeber();

        SetupCombatDirector(); // CombatDirect 초기화

        // [5] 전투 로직/틱 루프 시작 (서버와 동기화) 
        StartCoroutine(TickLoop_CombatDirector()); // CombatDirector 기반 틱 시작 
        StartCoroutine(BattleFlow());  // 연출/카메라/UI 흐름

        action?.Invoke();
    }
    private void SetupCombatDirector()
    {
        _combatDirector = new CombatDirector(_combatNetwork);
        _combatDirector.Init(_combatId);

        _snapshotApplier = new CombatSnapshotApplier(_actorObjects);
        _combatDirector.OnTickApplied += (snapshot, eventsThisTick) =>
        {
            _snapshotApplier.Apply(snapshot, eventsThisTick);
        };

        // CombatLog Event 처리
        _combatDirector.OnCombatEvent += HandleCombatEvent;

        // 전투 종료 콜백
        _combatDirector.OnBattleEnd += () =>
        {
            _battleEnded = true;
            _stageCleared = true;
        };

        _vfx = new CombatVfxPresenter(skillFxDb, _actorObjects, _actorMasterIds);
    }
    private IEnumerator TickLoop_CombatDirector()
    {
        Debug.Log("[BattleMap] TickLoop_CombatDirector 시작");

        while (!_battleEnded)
        {
            // 맵 이동 중이면 틱 안돌림
            if (!_combatTickEnabled || _isMapMoving)
            {
                yield return null;
                continue;
            }

            // CombatDirector가 Tick 처리
            yield return _combatDirector.Tick();

            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("[BattleMap] TickLoop_CombatDirector 종료");
    }
    private void HandleCombatEvent(CombatLogEventPb ev)
    {
        Debug.Log($"[HandleCombatEvent] {ev.Type.ToString()}");
        _vfx?.HandleEvent(ev);
        switch (ev.Type)
        {
            case "spawn":
                HandleSpawnEvent(ev);
                break;

            case "wave_cleared":
                HandleWaveClearEvent(ev);
                break;
        }
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
            Debug.LogWarning("[BattleMap] wave_cleared wave 파싱 실패");
            return;
        }
        foreach (var p in GetAlivePlayerActors())
            p.transform.DOKill(complete: true);

        _waitingReturnBeforeMapMove = true;
        _waveIndexForMove = wave;

        Debug.Log($"[BattleMap] wave_cleared 수신 wave={wave}");
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
        // [1] 1초동안 맵 이동 
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
        Debug.Log("전투 준비");
    }
    #region Battle Flow
    private IEnumerator BattleFlow()
    {
        var popup = BattleMapPopup.Instance;

        // [1] 전투 시작 연출
        yield return popup.StartCoroutine(popup.ShowStart());

        Debug.Log("유저 캐릭터들 등장 애니메이션");
        if (stageData.Batches.Count > 1)
        {
            _isMapMoving = true;
            yield return Move_Map();
            _isMapMoving = false;
        }
        _combatTickEnabled = true;

        while (!_battleEnded)
        {
            // 서버에서 wave_cleared 이벤트가 왔는지 체크
            if (_waitingReturnBeforeMapMove)
            {
                _waitingReturnBeforeMapMove = false;

                Debug.Log($"[BattleFlow] 웨이브 {_waveIndexForMove} 클리어 감지 → 플레이어 복귀 시작");

                // [1] 현재 웨이브 종료  플레이어 스폰지점으로 복귀
                yield return StartCoroutine(ReturnPlayersToSpawn());

                // 마지막 웨이브인지 확인
                bool isLastWave = IsLastWave(_waveIndexForMove);
                if (!isLastWave)
                {
                    // [2] 다음 웨이브를 위한 맵 이동
                    Debug.Log("[BattleFlow] 다음 웨이브로 맵 이동");
                    _isMapMoving = true;
                    yield return Move_Map();
                    _isMapMoving = false;
                }
                else
                {
                    // [3] 마지막 웨이브 -> 최종 복귀
                    Debug.Log("[BattleFlow] 마지막 웨이브 종료 -> End 복귀 시작");
                    yield return StartCoroutine(ReturnPlayersToSpawnEnd());

                    // 복귀 완료 → 전투 종료로 이동
                    break;
                }
            }

            // 매프레임 감시
            yield return null;
        }

        Debug.Log("[BattleFlow] BattleFlow 종료 감지 → FinishCombat 요청");

        // [3] 서버에 FinishCombat 요청  
        FinishCombatResponsePb result = null;
        bool done = false;

        yield return _combatNetwork.FinishCombatAsync(
            _combatId,
            res =>
            {
                if (!res.Ok)
                {
                    Debug.LogError("[BattleMap] FinishCombat 실패: " + res.Message);
                    done = true;
                    return;
                }

                result = res.Data;
                done = true;
            });

        if (!done || result == null)
        {
            Debug.LogError("[BattleMap] FinishCombat 결과 없음");
            yield break;
        }

        ApplyStageClearToClientProgress(result);

        popup.ShowResult(result);

        Debug.Log("[BattleMap] BattleFlow 전체 종료");
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
        _stageCleared = false;
        Debug.Log("[BattleMap] ReturnPlayersToSpawnEnd 완료");
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
            v.PlayIdle();

        Debug.Log("[BattleMap] ReturnPlayersToSpawn 완료");
    }
    private bool IsLastWave(int waveIndex)
    {
        return waveIndex >= stageData.Batches.Count - 1;
    }

    private void HandleSpawnEvent(CombatLogEventPb ev)
    {
        Debug.Log($"[SpawnEvent] raw actor={ev.Actor}, target={ev.Target}");

        if (long.TryParse(ev.Actor, out var actorId))
        {
            if (_actorObjects.TryGetValue(actorId, out var go))
            {
                if (!go.activeSelf)
                {
                    go.SetActive(true);
                    Debug.Log($"[BattleMap] HandleSpawnEvent: Actor {actorId} 활성화");
                }
            }
            else
            {
                Debug.LogWarning($"[BattleMap] HandleSpawnEvent: ActorId {actorId}에 해당하는 GameObject 없음");
            }
        }
    }


    #endregion 
    private bool AreAllPlayersAtSpawn()
    {
        const float tolerance = 0.2f; // 오차 허용 범위

        foreach (var kv in _actorObjects)
        {
            long actorId = kv.Key;
            GameObject go = kv.Value;

            // 플레이어 팀만
            if (!_actorTeams.TryGetValue(actorId, out var team) || team != CombatTeam.Player)
                continue;

            var view = go.GetComponent<CombatActorView>();
            if (view == null)
                continue;

            // 죽은 애는 제외
            if (view.Hp <= 0)
                continue;

            if (!_playerSpawnPos.TryGetValue(actorId, out var spawnPos))
                continue;

            float dist = Vector3.Distance(view.transform.position, spawnPos);
            if (dist > tolerance)
            {
                // 하나라도 아직 멀면 false
                return false;
            }
        }

        // 살아있는 플레이어들이 전부 스폰 근처에 있음
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

            var view = go.GetComponent<CombatActorView>();
            if (view == null)
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
            Debug.LogWarning("[BattleMap] 전투 종료 후 스킬 사용 불가");
            onResult?.Invoke(false);
            return;
        }

        Debug.Log($"[ 스킬 실행 ] {actorId} : {skillId}");
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
            Debug.LogError("[BattleMap] 스킬 요청 실패: " + response.Message);
            onResult?.Invoke(false);
            yield break;
        }

        Debug.Log("[BattleMap] 스킬 요청 성공");
        onResult?.Invoke(true);
    }

}
