using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager Instance { get; private set; }

    private Dictionary<string, Object> _cache = new Dictionary<string, Object>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public async Task<T> LoadAsync<T>(string key) where T : Object
    {
        if (_cache.ContainsKey(key))
            return _cache[key] as T;

        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _cache[key] = handle.Result;
            return handle.Result;
        }
        else
        {
            Debug.LogError($"[AddressableManager] Failed to load asset with key: {key}");
            return null;
        }
    }
    public void Release(string key)
    {
        if (_cache.ContainsKey(key))
        {
            Addressables.Release(_cache[key]);
            _cache.Remove(key);
        }
    }
    public void ClearAll()
    {
        foreach (var item in _cache.Values)
            Addressables.Release(item);
        _cache.Clear();
    }

}
