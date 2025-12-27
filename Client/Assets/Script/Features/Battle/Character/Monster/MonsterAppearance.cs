using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MonsterAppearance : MonoBehaviour
{
    [SerializeField] private Transform modelParent;
    private GameObject monster;
    public async void Set(int monsterId, Action onLoaded = null)
    {
        await GetModelObj(monsterId, onLoaded);
    }
    private async Task GetModelObj(int monsterId, Action onLoaded = null)
    {
        MonsterCache monsterCache = MonsterCache.Instance;
        string modelKey = monsterCache.MonstersById[monsterId].ModelKey;
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(modelKey);
        await handle.Task;
        GameObject prefab = handle.Result;
        monster = Instantiate(prefab, modelParent);
        if (onLoaded != null)
            onLoaded.Invoke();
    }
}
