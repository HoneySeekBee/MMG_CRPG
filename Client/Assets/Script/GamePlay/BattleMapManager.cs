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

public class BattleMapManager : MonoBehaviour
{
    public static BattleMapManager Instance { get; private set; }

    // Network
    private CombatNetwork _combatNetwork;
    private long _combatId;
    private StartCombatResponsePb _combatStart;

    // Map / Wave
    private Dictionary<int, List<MonsterBase>> MonstersByWaves = new Dictionary<int, List<MonsterBase>>();
    [SerializeField] private Dictionary<int, BatchSlot> monsterSlotByIndex = new();
    private readonly Dictionary<long, GameObject> _actorObjects = new(); //  ActorId → GameObject
    private readonly List<long> _enemyActorIds = new();

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
    public IEnumerator Set_BattleMap()
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

        int currentWave = 0;
        WavePb[] waves = stageData.Waves.OrderBy(x => x.Index).ToArray();

        Vector3 goalPos = new Vector3(0, 1.5f, 0);

        for (int b = 1; b < stageData.Batches.Count; b++)
        {
            goalPos.x = b * 20 - 5;
            UserPartyObj.transform.DOMove(goalPos, 1f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(1f);

            bool isBattle = (currentWave < waves.Length && waves[currentWave].BatchNum == b);

            if (isBattle)
            {
                Debug.Log($"웨이브 {currentWave} 시작");

                // 해당 Wave 몬스터 활성화
                ActivateServerActorsForWave(waves[currentWave].Index);

                // 서버 틱 폴링 시작
                yield return StartCoroutine(PollCombat());

                currentWave++;
            }
        }
    }
    private IEnumerator PollCombat()
    {
        while (!_battleEnded)
        {
            yield return _combatNetwork.GetLogAsync(
                _combatId,
                _logCursor,
                size: 50,
                res =>
                {
                    if (!res.Ok) return;

                    CombatLogPagePb page = res.Data;

                    // cursor 업데이트
                    if (!string.IsNullOrEmpty(page.NextCursor))
                        _logCursor = page.NextCursor;

                    foreach (var ev in page.Items)
                        HandleCombatEvent(ev);

                    // 종료 처리
                    if (page.Items.Any(x => x.Type == "stage_cleared"))
                        _battleEnded = true;
                });

            yield return new WaitForSeconds(0.1f);
        }
    }
    private void HandleCombatEvent(CombatLogEventPb ev)
    {
        switch (ev.Type)
        {
            case "spawn":
                HandleSpawn(ev);
                break;

            case "hit":
                ApplyDamage(ev);
                break;

            case "death":
                HandleDeath(ev);
                break;

            case "wave_cleared":
                int wave = GetWaveIndexFromExtra(ev);
                Debug.Log($"웨이브 완료: {wave}");
                break;

            case "stage_cleared":
                Debug.Log("전투 종료");
                break;
        }
    }
    private void HandleSpawn(CombatLogEventPb ev)
    {
        long actorId = long.Parse(ev.Actor);
        int wave = GetWaveIndexFromExtra(ev);

        Debug.Log($"[Spawn] enemy {actorId} wave={wave}");

        if (_actorObjects.TryGetValue(actorId, out var go))
        {
            go.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[Spawn] actorId={actorId} 에 해당하는 오브젝트가 없습니다.");
        }
    }
    private int GetWaveIndexFromExtra(CombatLogEventPb ev)
    {
        if (ev.Extra == null)
            return -1;

        // ev.Extra.Fields["wave"] 로 접근해야 함
        if (ev.Extra.Fields.TryGetValue("wave", out Value v))
        {
            // 서버에서 string으로 넣었으니 StringValue 사용
            if (int.TryParse(v.StringValue, out int wave))
                return wave;
        }

        return -1;
    } 
    private void ApplyDamage(CombatLogEventPb ev)
    {
        if (!_actorObjects.TryGetValue(long.Parse(ev.Target), out var obj))
            return;

        var view = obj.GetComponent<CombatActorView>();
        if (view == null)
            return;

        int dmg = ev.Damage ?? 0;
        bool isCrit = ev.Crit ?? false;

        view.ApplyDamage(dmg, isCrit);
    }

    private void HandleDeath(CombatLogEventPb ev)
    {
        long actorId = long.Parse(ev.Actor);

        if (_actorObjects.TryGetValue(actorId, out GameObject go))
        {
            go.SetActive(false);
        }

        Debug.Log($"[DEATH] actor={actorId}");
    }
    #endregion

    private void SetupActorsFromSnapshot(CombatInitialSnapshotPb snapshot)
    {
        _actorObjects.Clear();
        _enemyActorIds.Clear();

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
                _enemyActorIds.Add(actor.ActorId);
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
        if (!MonstersByWaves.ContainsKey(waveIndex)) return;

        foreach (var monster in MonstersByWaves[waveIndex])
            monster.gameObject.SetActive(true); 
    }

}
