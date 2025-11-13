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

public class BattleMapManager : MonoBehaviour
{
    public static BattleMapManager Instance { get; private set; }
    private StagePb stageData;
    [SerializeField] private GameObject MonsterBasePrefab;
    private Dictionary<int, List<MonsterBase>> MonstersByWaves = new Dictionary<int, List<MonsterBase>>();
    [SerializeField] private GameObject UserPartyObj;

    [Header("몬스터 관련")]
    [SerializeField] private PartySlot[] monsterSlots;
    [SerializeField] private Dictionary<int, BatchSlot> monsterSlotByIndex = new();

    // 전투 시작 응답 / 서버 엑터 정보
    private CombatNetwork _combatNetwork;
    private StartCombatResponsePb _combatStart;
    private long _combatId;
    // 서버에서 받은 Actor 정보 -> 실제 Unity 오브젝트로 매핑하기 
    private readonly Dictionary<long, GameObject> _actorObjects = new(); //  ActorId → GameObject

    // 서버로 받은 적 
    private readonly List<long> _enemyActorIds = new();

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
    public IEnumerator Set_BattleMap()
    {
        MonstersByWaves.Clear();
        Set_MonsterSlot();

        // [1] 서버에 전투 시작 요청
        int stageId = LobbyRootController.Instance._currentStage.Id;
        long battleId = LobbyRootController.Instance._currentBattleId; // 네가 FormationId로 쓰는 값

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
        PartySetManager party = PartySetManager.Instance;

        // [1] 게임 시작 
        yield return popup.StartCoroutine(popup.ShowStart());
        Vector3 goalPos = Vector3.zero;
        goalPos.y = 1.5f;
        int currentWave = 0;
        WavePb[] waves = stageData.Waves.OrderBy(x => x.Index).ToArray();

        for (int b = 1; b < stageData.Batches.Count; b++)
        {
            // [2] 다음 장소로 이동 
            goalPos.x = b * 20 - 5;
            UserPartyObj.transform.DOMove(goalPos, 1f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(1);
            Debug.Log($"웨이브 갯수 {waves.Length} {currentWave}");
            bool isBattle = (currentWave < waves.Length && waves[currentWave].BatchNum == b);
            if (isBattle)
            {
                Debug.Log("몬스터 생성");
                foreach (var monster in MonstersByWaves[waves[currentWave].Index])
                {
                    monster.gameObject.SetActive(true);
                    monster.transform.position = monsterSlotByIndex[monster.MonsterData.Id].transform.position;
                }
                currentWave++;
                // 여기서 전투를 해야한다. 
                yield return new WaitForSeconds(5);
            }
            if (currentWave == waves.Length)
                Debug.Log("게임 종료 ");
        }
    }
    #endregion

    private void SetupActorsFromSnapshot(CombatInitialSnapshotPb snapshot)
    {
        _actorObjects.Clear();
        _enemyActorIds.Clear();

        foreach (var actor in snapshot.Actors)
        {
            string enemy = actor.Team == 1 ? "Enemy" : "Character";
            Debug.Log($"[{enemy}] {actor.MasterId} ");
            string name = actor.Team == 1 ? MonsterCache.Instance.MonstersById[(int)actor.MasterId].Name : CharacterCache.Instance.DetailById[(int)actor.MasterId].Name;
            Debug.Log($"[{enemy}] {name} : {actor.Hp} {actor.WaveIndex} {actor.X} {actor.Z}");
        }

        foreach (var actor in snapshot.Actors)
        {
            GameObject go = GetPrefabByModelCode(actor.MasterId, actor.Team, actor.WaveIndex);
            go.transform.parent = this.transform;
            Vector3 worldPos = new Vector3(actor.X, 0, actor.Z); // 필요하면 offset 추가 

            go.transform.position = worldPos;
            if (actor.Team == 1)
                go.SetActive(false);

            if (actor.Team == 1)
            {
                var monsterBase = go.GetComponent<MonsterBase>();
                if (monsterBase != null)
                {
                    //  monsterBase.InitFromServer(actor); // 새로운 초기화 함수 만들기 추천
                }
                _enemyActorIds.Add(actor.ActorId);
            }
            else
            {
                // 플레이어 유닛 초기화
                //var hero = go.GetComponent<HeroBase>();
                //if (hero != null)
                //{
                //    hero.InitFromServer(actor);
                //}
            }

            _actorObjects[actor.ActorId] = go;
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
            CharacterAppearance appearance = character.GetComponent<CharacterAppearance>();
            appearance.Set(CharacterCache.Instance.CharacterModelById[(int)actorId], true);

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
        foreach (var kv in _actorObjects)
        {
            kv.Value.SetActive(true);
        }
    }

}
