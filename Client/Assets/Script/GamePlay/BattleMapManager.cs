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

public class BattleMapManager : MonoBehaviour
{
    public static BattleMapManager Instance { get; private set; }

    // Network
    private CombatNetwork _combatNetwork;
    private CombatDirector _combatDirector;

    private long _combatId; 
    private StartCombatResponsePb _combatStart;

    // Map / Wave 
    private readonly Dictionary<long, GameObject> _actorObjects = new();
    private readonly Dictionary<long, CombatTeam> _actorTeams = new();
    private readonly Dictionary<long, ActorLastState> _lastStates = new();
    private readonly Dictionary<long, Vector3> _playerSpawnPos = new();

    [SerializeField] private Dictionary<int, BatchSlot> monsterSlotByIndex = new();
    private readonly List<long> _enemyActorIds = new();
    private readonly Dictionary<long, int> _actorWaveIndex = new();
    private class ActorLastState
    {
        public Vector3 Pos;
        public int Hp;
        public bool Dead;
    }
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

    public IEnumerator Set_BattleMap(Action action)
    {
        Set_MonsterSlot();

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
        SetupActorsFromSnapshot(_combatStart.Snapshot);

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

        // Snapshot 적용
        _combatDirector.OnTickApplied += ApplyTickSnapshot;

        // CombatLog Event 처리
        _combatDirector.OnCombatEvent += HandleCombatEvent;

        // 전투 종료 콜백
        _combatDirector.OnBattleEnd += () =>
        {
            _battleEnded = true;
            _stageCleared = true;
        };
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
        switch (ev.Type)
        {
            case "spawn":
                HandleSpawnEvent(ev);
                break;

            case "skill_cast":
                HandleSkillCastEvent(ev);
                break;

            case "hit":
                HandleHitEvent(ev); // 기존 TickLoop에 있던 hit 로직 그대로 유지 가능
                break;

            case "wave_cleared":
                HandleWaveClearEvent(ev);
                break; 
        }
    }
    private void HandleSkillCastEvent(CombatLogEventPb ev)
    {
        if (!long.TryParse(ev.Actor, out long casterId))
            return;

        if (!_actorObjects.TryGetValue(casterId, out var go))
            return;

        var view = go.GetComponent<CombatActorView>();
        if (view == null)
            return;

        int skillId = -1;
        if (ev.Extra != null && ev.Extra.Fields.TryGetValue("skillId", out var v))
        {
            if (v.KindCase == Google.Protobuf.WellKnownTypes.Value.KindOneofCase.NumberValue)
                skillId = (int)v.NumberValue;
            else if (v.KindCase == Google.Protobuf.WellKnownTypes.Value.KindOneofCase.StringValue)
                int.TryParse(v.StringValue, out skillId);
        }

        Debug.Log($"[BattleMap] SkillCast → caster={casterId}, skillId={skillId}");

        // 공격 애니메이션 실행
        view.PlayAttack(false);

        // 나중에: 스킬 FX / 사운드 / 카메라 흔들림 추가 가능
    }
    private void HandleHitEvent(CombatLogEventPb ev)
    {
        if (!long.TryParse(ev.Target, out var targetId))
            return;

        if (!_actorObjects.TryGetValue(targetId, out var go))
            return;

        var view = go.GetComponent<CombatActorView>();

        bool isCrit = ev.Crit ?? false;
        int dmg = ev.Damage?? 0;

        Debug.Log($"[BattleMap] Hit → target={targetId}, dmg={dmg}, crit={isCrit}");

        view.PlayHitFx(isCrit);

        // 나중에 데미지 텍스트 붙이면 됨
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

                // [1] 현재 웨이브 종료 → 플레이어 스폰지점으로 복귀
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
                    // [3] 마지막 웨이브 → 최종 복귀
                    Debug.Log("[BattleFlow] 마지막 웨이브 종료 → End 복귀 시작");
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
        if (_endReturnDone)
            yield break;

        var players = GetAlivePlayerActors(); // 죽은 애도 끌어올 거면 별도 함수로 확장

        if (players.Count == 0)
        {
            _endReturnDone = true;
            yield break;
        }

        float duration = 1.0f;

        foreach (var v in players)
        {
            v.PlayMove();

            Vector3 target = new Vector3(v.SpawnX, 0f, v.SpawnZ);
            v.transform.DOMove(target, duration);
        }

        yield return new WaitForSeconds(duration);

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

        float duration = 1.0f;

        foreach (var v in players)
        {
            v.PlayMove();
            Vector3 target = new Vector3(v.SpawnX, 0f, v.SpawnZ);
            v.transform.DOMove(target, duration);
        }

        yield return new WaitForSeconds(duration);

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
        // ev.Actor 에 spawn된 ActorId가 들어온다고 가정
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


    private void ApplyTickSnapshot(CombatSnapshotPb snapshot, IList<CombatLogEventPb> eventsThisTick)
    {
        foreach (var a in snapshot.Actors)
        {
            if (!_actorObjects.TryGetValue(a.ActorId, out var go))
                continue;

            var view = go.GetComponent<CombatActorView>();
            if (view == null)
                continue;
            if (!a.Dead && !go.activeSelf)
            {
                go.SetActive(true);
                Debug.Log($"[BattleMap] Snapshot 기반 활성화: actorId={a.ActorId}");
            }

            if (!_lastStates.TryGetValue(a.ActorId, out var prev))
            {
                prev = new ActorLastState
                {
                    Pos = view.transform.position,
                    Hp = view.Hp,
                    Dead = false
                };
            }

            // 위치 보간/이동
            var newPos = new Vector3(a.X, 0, a.Z);
            float moveDist = Vector3.Distance(prev.Pos, newPos);

            view.transform.position = newPos;

            bool isMoving = moveDist > 0.01f && !a.Dead;
            if (isMoving)
                view.PlayMove();
            else if (!a.Dead)
                view.PlayIdle();

            // HP/피격 처리
            int prevHp = prev.Hp;
            view.SetHp(a.Hp);
            int damage = Mathf.Max(0, prevHp - a.Hp);

            string myId = a.ActorId.ToString();

            // 이 틱에서 나와 관련된 이벤트들
            bool didAttackThisTick = eventsThisTick.Any(ev => ev.Type == "hit" && ev.Actor == myId);
            bool gotHitThisTick = eventsThisTick.Any(ev => ev.Type == "hit" && ev.Target == myId);
            bool isCrit = eventsThisTick.Any(ev => ev.Type == "hit" && ev.Target == myId && (ev.Crit ?? false));
              
            // 죽음 처리
            if (!prev.Dead && a.Dead)
                view.OnDie();

            prev.Pos = newPos;
            prev.Hp = a.Hp;
            prev.Dead = a.Dead;
            _lastStates[a.ActorId] = prev;
        }
    }
    #endregion
    private void SetupActorsFromSnapshot(CombatInitialSnapshotPb snapshot)
    {
        _actorObjects.Clear();
        _enemyActorIds.Clear();
        _actorTeams.Clear();
        _actorWaveIndex.Clear();
        _lastStates.Clear();
        _playerSpawnPos.Clear();

        foreach (var actor in snapshot.Actors)
        {
            GameObject go = GetPrefabByModelCode(actor.MasterId, actor.Team, actor.WaveIndex);
            CombatActorView actorView = go.GetComponent<CombatActorView>();
            actorView.InitFromServer(actor.ActorId, actor.Team, actor.Hp);
            go.transform.parent = PartySetManager.Instance.transform;

            Vector3 worldPos = new Vector3(actor.X, 0, actor.Z);
            go.transform.position = worldPos;

            if (actor.Team == 1)
            {
                // 적은 기본적으로 비활성화해두고,
                // 서버 spawn 이벤트에서 켜준다.
                go.SetActive(false);
                _enemyActorIds.Add(actor.ActorId);
            }
            else if (actor.Team == 0)
            { 
                _playerSpawnPos[actor.ActorId] = worldPos;
                actorView.SetSpawnPosition(worldPos);

                // 스킬 아이콘 만들어주기 
                int characterId = (int)actor.MasterId;
                int level = GameState.Instance.CurrentUser.UserCharactersDict[characterId].Level;
                BattleMapPopup.Instance.CreateSkillButton(characterId, actor.ActorId, level);
            }

            _actorObjects[actor.ActorId] = go;
            _actorTeams[actor.ActorId] = (CombatTeam)actor.Team;
            _actorWaveIndex[actor.ActorId] = actor.WaveIndex;
        }

        Debug.Log($"[BattleMap] SetupActorsFromSnapshot 완료. Actors={snapshot.Actors.Count}");
    }
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
    private GameObject GetPrefabByModelCode(long actorId, int team, int waveIndex = 0)
    {
        if (actorId == 0) return null;
        // TODO: 실제로는 Addressable/ScriptableObject 테이블 써서 매핑
        if (team == 0)
        {
            GameObject character = PartySetManager.Instance.GetCharacterObject();
            // 유저 파티
            Debug.Log($"[character ] {actorId} {CharacterCache.Instance.DetailById[(int)actorId].Name}");
            CharacterBase chaBase = character.GetComponent<CharacterBase>();
            chaBase.Set(CharacterCache.Instance.CharacterModelById[(int)actorId], true);

            return character;
        }
        else
        {
            GameObject monster = Instantiate(MonsterBasePrefab, PartySetManager.Instance.transform);
            MonsterBase monsterBase = monster.GetComponent<MonsterBase>();
            MonsterPb monsterPb = MonsterCache.Instance.MonstersById[(int)actorId];
            monsterBase.Set(monsterPb);

            monster.SetActive(false);
            return monster;
        }
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
    public void RequestSkill(long actorId, int skillId)
    {
        if (_battleEnded)
        {
            Debug.LogWarning("[BattleMap] 전투 종료 후 스킬 사용 불가");
            return;
        }

        StartCoroutine(RequestSkillRoutine(actorId, skillId));
    }

    private IEnumerator RequestSkillRoutine(long actorId, int skillId)
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

        // OK 여부 체크
        if (!response.Ok)
        {
            Debug.LogError("[BattleMap] 스킬 요청 실패: " + response.Message);
            yield break;
        }

        Debug.Log("[BattleMap] 스킬 요청 성공");
    }
}
