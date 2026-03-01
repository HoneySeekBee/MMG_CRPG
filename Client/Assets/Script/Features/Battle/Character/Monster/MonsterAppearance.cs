using Game.Logging;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class MonsterAppearance : MonoBehaviour
{
    [SerializeField] private Transform modelParent;
    private GameObject monster;
    public async Task Set(int monsterId)
    {
        try
        {
            string modelKey = MonsterCache.Instance.MonstersById[monsterId].ModelKey;
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(modelKey);
            await handle.Task;
            monster = Instantiate(handle.Result, modelParent);
        }
        catch (Exception e)
        {
            GameLogger.Error($"[MonsterAppearance] Set failed: {e.Message}");
        }
    }
}
