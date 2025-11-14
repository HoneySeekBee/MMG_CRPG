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

public class BattleMapManager : MonoBehaviour
{
    public static BattleMapManager Instance { get; private set; }

    // Network
    private CombatNetwork _combatNetwork;
    private long _combatId;
    private int _clientTick;
    private StartCombatResponsePb _combatStart;

    // Map / Wave
    private Dictionary<int, List<MonsterBase>> MonstersByWaves = new Dictionary<int, List<MonsterBase>>();
    [SerializeField] private Dictionary<int, BatchSlot> monsterSlotByIndex = new();
    private readonly Dictionary<long, GameObject> _actorObjects = new(); //  ActorId → GameObject
    private readonly List<long> _enemyActorIds = new();
    private readonly Dictionary<long, CombatTeam> _actorTeams = new();
    private readonly Dictionary<long, int> _actorWaveIndex = new();
    private readonly Dictionary<long, ActorLastState> _lastStates = new();
    private class ActorLastState
    {
        public Vector3 Pos;
        public int Hp;
        public bool Dead;
    }
    [SerializeField] private GameObject MonsterBasePrefab;
    [SerializeField] private GameObject UserPartyObj;
     
    private StagePb stageData;
    [Header("몬스터 관련")]
    [SerializeField] private PartySlot[] monsterSlots;
    private string _logCursor = "";
    private bool _battleEnded = false;


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
        MonstersByWaves.Clear();
        Set_MonsterSlot();

        // [1] 서버에 전투 시작 요청
        int stageId = LobbyRootController.Instance._currentStage.Id;
        long battleId = LobbyRootController.Instance._currentBattleId;  

        _combatStart = null;
        Debug.Log($"전투 데이터 요청 {stageId} || {battleId}");

        yield return _combatNetwork.StartCombatAsync(stageId, battleId, res =>
        {
            if (!res.Ok)
            {
                Debug.LogError($"[BattleMap] StartCombat 실패: {res.Message}");
                // 팝업은 CombatNetwork 안에서 이미 띄웠으니 여기선 흐름 정리만
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

        // [2] 맵을 생성해야한다. 
        yield return Set_Map().AsCoroutine();

        // [3] 서버 스냅샷 기반 유닛 생산 
        SetupActorsFromSnapshot(_combatStart.Snapshot);

        // [4] UI 연계  
        PartyMemeber();

        // [5] 배틀 플로우 시작 ( 맵 이동 + 전투 연출 ) 
        StartCoroutine(BattleFlow());
        action.Invoke();
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

        foreach (var batch in stageData.Batches)
        {
            var UnitObj = await AddressableManager.Instance.LoadAsync<GameObject>(batch.UnitKey);
            var EnvObj = await AddressableManager.Instance.LoadAsync<GameObject>(batch.EnvKey);

            GameObject unit = Instantiate(UnitObj, this.transform);
            GameObject env = Instantiate(EnvObj, unit.transform);

            unit.transform.position = new Vector3(20 * (batch.BatchNum - 1), 0, 0);
        }
    }
    private void PartyMemeber()
    {
        Debug.Log("전투 준비");
    }
    #region Battle Flow
    private IEnumerator BattleFlow()
    {
        BattleMapPopup popup = BattleMapPopup.Instance; 

        // [1] 게임 시작 
        yield return popup.StartCoroutine(popup.ShowStart());

        // (1) 파티 구성원들의 애니메이션 
        Debug.Log("유저 캐릭터들 등장 애니메이션");

        // (2) 맵 이동


        int currentWave = 0;
        WavePb[] waves = stageData.Waves.OrderBy(x => x.Index).ToArray();

        Vector3 goalPos = new Vector3(0, 1.5f, 0); 
        for (int b = 1; b < stageData.Batches.Count; b++)
        { 
            yield return new WaitForSeconds(1f);

            bool isBattle = (currentWave < waves.Length && waves[currentWave].BatchNum == b);

            if (isBattle)
            {
                var waveIndex = waves[currentWave].Index;
                Debug.Log($"웨이브 {waveIndex} 시작"); 

                // 해당 Wave 몬스터 활성화
                ActivateServerActorsForWave(waves[currentWave].Index);

                // 서버 틱 폴링 시작
                yield return StartCoroutine(TickLoop(waveIndex));

                currentWave++;
            } 
        }
    }
    private IEnumerator TickLoop(int waveIndex)
    {
        bool waveEnded = false;

        while (!waveEnded && !_battleEnded)
        {
            yield return _combatNetwork.TickAsync(
                _combatId,
                _clientTick,
                res =>
                {
                    if (!res.Ok)
                    {
                        Debug.Log("[TickLoop Error] " + res.Message);
                        return;
                    }

                    var snapshot = res.Data.Snapshot;
                    var events = res.Data.Events;
                    ApplyTickSnapshot(snapshot, events);

                    // 패배 판정용: 플레이어 살아있는지
                    bool anyPlayerAlive = snapshot.Actors != null &&
                                          snapshot.Actors.Any(a =>
                                              _actorTeams.TryGetValue(a.ActorId, out var team) &&
                                              team == CombatTeam.Player &&
                                              !a.Dead);

                    if (!anyPlayerAlive)
                    {
                        _battleEnded = true;
                        waveEnded = true;
                        Debug.Log("[BattleMap] 전투 종료: lose");
                        _clientTick++;
                        return;
                    }

                    // 서버에서 온 CombatLog/이벤트 확인 
                    foreach (var ev in events)
                    {
                        switch (ev.Type)
                        {
                            case "wave_cleared":
                                {
                                    // wave 번호 확인
                                    int evWaveIndex = -1;
                                    if (ev.Extra != null && ev.Extra.Fields.TryGetValue("wave", out var v))
                                    {
                                        // 서버에서 wave 인덱스를 string으로 넣었으면:
                                        if (v.KindCase == Value.KindOneofCase.StringValue)
                                            int.TryParse(v.StringValue, out evWaveIndex);
                                        // number로 넣었으면:
                                        else if (v.KindCase == Value.KindOneofCase.NumberValue)
                                            evWaveIndex = (int)v.NumberValue;
                                    }

                                    Debug.Log($"[BattleMap] wave_cleared 이벤트 수신. evWaveIndex={evWaveIndex}, current={waveIndex}");

                                    // 현재 틱루프가 처리 중인 웨이브와 같으면 종료
                                    if (evWaveIndex == waveIndex)
                                    {
                                        waveEnded = true;
                                        Debug.Log($"[BattleMap] 웨이브 {waveIndex} 종료 (서버 wave_cleared)");
                                    }
                                    break;
                                }

                            case "stage_cleared":
                                {
                                    _battleEnded = true;
                                    waveEnded = true;
                                    Debug.Log("[BattleMap] 스테이지 클리어 (stage_cleared)");
                                    break;
                                }

                            case "hit":
                                {
                                    // 필요하면 여기서도 이펙트 처리 가능 
                                    break;
                                }
                        }
                    }

                    _clientTick++;
                });

            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("[BattleMap] TickLoop 종료");
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

            if (!_lastStates.TryGetValue(a.ActorId, out var prev))
            {
                prev = new ActorLastState
                {
                    Pos = view.transform.position,
                    Hp = view.Hp,
                    Dead = false
                };
            }

            var newPos = new Vector3(a.X, 0, a.Z);
            float moveDist = Vector3.Distance(prev.Pos, newPos);

            view.transform.position = newPos;

            bool isMoving = moveDist > 0.01f && !a.Dead;
            if (isMoving)
                view.PlayMove();
            else if (!a.Dead)
                view.PlayIdle();

            int prevHp = prev.Hp;
            view.SetHp(a.Hp);
            int damage = Mathf.Max(0, prevHp - a.Hp);

            string myId = a.ActorId.ToString();

            //  이 틱에서 나와 관련된 이벤트들
            bool didAttackThisTick = eventsThisTick.Any(ev => ev.Type == "hit" && ev.Actor == myId);
            bool gotHitThisTick = eventsThisTick.Any(ev => ev.Type == "hit" && ev.Target == myId);
            bool isCrit = eventsThisTick.Any(ev => ev.Type == "hit" && ev.Target == myId && (ev.Crit ?? false));

            if (didAttackThisTick && !a.Dead)
                view.PlayAttack(isCrit);

            if (damage > 0 && gotHitThisTick)
                view.PlayHitFx(isCrit);

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

        foreach (var actor in snapshot.Actors)
        {
            GameObject go = GetPrefabByModelCode(actor.MasterId, actor.Team, actor.WaveIndex);
            CombatActorView actorView = go.GetComponent<CombatActorView>();
            actorView.InitFromServer(actor.ActorId, actor.Team, actor.Hp);
            go.transform.parent = this.transform;

            Vector3 worldPos = new Vector3(actor.X, 0, actor.Z); // 필요하면 offset 추가 
            go.transform.position = worldPos;

            if (actor.Team == 1)
                go.SetActive(false);

            if (actor.Team == 1)
            { 
                _enemyActorIds.Add(actor.ActorId);
            } 
            _actorObjects[actor.ActorId] = go;
            _actorTeams[actor.ActorId] = (CombatTeam)actor.Team;
            _actorWaveIndex[actor.ActorId] = actor.WaveIndex;
        }
        Debug.Log($"[BattleMap] SetupActorsFromSnapshot 완료. Actors={snapshot.Actors.Count}");
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
            GameObject monster = Instantiate(MonsterBasePrefab, this.transform);
            MonsterBase monsterBase = monster.GetComponent<MonsterBase>();
            MonsterPb monsterPb = MonsterCache.Instance.MonstersById[(int)actorId];
            monsterBase.Set(monsterPb);

            if (MonstersByWaves.ContainsKey(waveIndex) == false || MonstersByWaves[waveIndex] == null)
                MonstersByWaves[waveIndex] = new List<MonsterBase>();

            MonstersByWaves[waveIndex].Add(monsterBase);
            monster.SetActive(false);
            return monster;
        }
    }
    private void ActivateServerActorsForWave(int waveIndex)
    {
        if (!MonstersByWaves.ContainsKey(waveIndex)) return;

        Debug.Log($"Wave {waveIndex} 활성화 : {MonstersByWaves[waveIndex].Count}");
        foreach (var monster in MonstersByWaves[waveIndex])
            monster.gameObject.SetActive(true); 
    }
    private List<CombatActorView> GetAlivePlayerActors()
    {
        var result = new List<CombatActorView>();

        foreach (var kv in _actorObjects)
        {
            long actorId = kv.Key;
            GameObject go = kv.Value;

            // 팀 정보 없으면 스킵
            if (!_actorTeams.TryGetValue(actorId, out var team))
                continue;

            // 플레이어 팀만
            if (team != CombatTeam.Player)
                continue;

            var view = go.GetComponent<CombatActorView>();
            if (view == null)
                continue;

            // 살아있는 애만 (Hp > 0 기준, 필요하면 view.IsDead 같은 플래그 써도 됨)
            if (view.Hp <= 0)
                continue;

            result.Add(view);
        }

        return result;
    }
}
