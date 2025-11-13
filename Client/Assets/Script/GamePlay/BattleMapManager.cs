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
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public IEnumerator Set_BattleMap()
    {
        MonstersByWaves.Clear();
        Set_MonsterSlot();
        // [1] 맵을 생성해야한다. 
        yield return Set_Map().AsCoroutine();
        // [2] 웨이브마다 적들을 생성한다. 
        MonsterCache monsterCache = MonsterCache.Instance;
        foreach (var w in stageData.Waves)
        {
            foreach (var e in w.Enemies)
            {
                GameObject monster = Instantiate(MonsterBasePrefab, this.transform);
                MonsterBase monsterBase = monster.GetComponent<MonsterBase>();
                monsterBase.Set(e);
                if (MonstersByWaves.ContainsKey(w.Index) == false || MonstersByWaves[w.Index] == null)
                    MonstersByWaves[w.Index] = new List<MonsterBase>();
                MonstersByWaves[w.Index].Add(monsterBase);
                monster.SetActive(false);
            }
        }
        // [3] 유저 파티를 생성해야한다. 
        PartySetManager.Instance.Initialize(LobbyRootController.Instance._currentBattleId, PartyMemeber, true);

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
            if(isBattle)
            {
                Debug.Log("몬스터 생성");
                foreach (var monster in MonstersByWaves[waves[currentWave].Index])
                {
                    monster.gameObject.SetActive(true);
                    monster.transform.position = monsterSlotByIndex[monster.MonsterData.Slot].transform.position;
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
}
