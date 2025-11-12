using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extentions;
using System.Threading.Tasks;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BattleMapManager : MonoBehaviour
{
    public static BattleMapManager Instance { get; private set; }
    private StagePb stageData;
    [SerializeField] private GameObject MonsterBasePrefab;
    private Dictionary<int, List<GameObject>> MonstersByWaves = new Dictionary<int, List<GameObject>>();
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
        // [1] 맵을 생성해야한다. 
        yield return Set_Map().AsCoroutine();
        // [2] 유저 파티를 생성해야한다. 
        PartySetManager.Instance.Initialize(LobbyRootController.Instance._currentBattleId, PartyMemeber, true);
        // [3] 웨이브마다 적들을 생성한다. 
        MonsterCache monsterCache = MonsterCache.Instance; 

        foreach (var w in stageData.Waves)
        {
            foreach (var e in w.Enemies)
            {
                GameObject monster = Instantiate(MonsterBasePrefab, this.transform);  
                monster.GetComponent<MonsterAppearance>().Set(e.EnemyCharacterId);
                Debug.Log($"[{stageData.Name}] 웨이브 {w.Index} / 적 {monsterCache.MonstersById[e.EnemyCharacterId].Name}을 {e.Slot}");
                if (MonstersByWaves.ContainsKey(w.Index) == false || MonstersByWaves[w.Index] == null)
                    MonstersByWaves[w.Index] = new List<GameObject>();
                MonstersByWaves[w.Index].Add(monster);
                monster.SetActive(false);
            }
        }
    }

    private async Task Set_Map()
    {
        stageData = LobbyRootController.Instance._currentStage;

        foreach (var batch in stageData.Batches)
        {
            var UnitObj = await AddressableManager.Instance.LoadAsync<GameObject>(batch.UnitKey);
            var EnvObj = await AddressableManager.Instance.LoadAsync<GameObject>(batch.EnvKey);
            GameObject unit = Instantiate(UnitObj);
            GameObject env = Instantiate(EnvObj, unit.transform);
            unit.transform.position = new Vector3(20 * (batch.BatchNum - 1), 0, 0);
        }
    }
    private void PartyMemeber()
    {
        Debug.Log("전투 준비");
    }
}
