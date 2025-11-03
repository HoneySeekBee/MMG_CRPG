using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace Lobby
{
    public class UIPrefabPool : MonoBehaviour
    {
        public static UIPrefabPool Instance { get; private set; }
        // 프리펩 원본 
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _prefabHandles = new();
        // 풀(비활성 인스턴스)
        private readonly Dictionary<string, Queue<GameObject>> _pools = new();

        private readonly Dictionary<string, Task<GameObject>> _inflight = new();
        private readonly Dictionary<string, GameObject> _active = new();
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // 패널 / 위젯 보여주기 
        public async Task<GameObject> ShowAsync(string key, Transform parent)
        {
            if (_active.TryGetValue(key, out var activeGo) && activeGo)
            {
                activeGo.transform.SetParent(parent, false);
                activeGo.SetActive(true);
                return activeGo;
            }

            // 1) 풀에 남아있으면 바로 재사용
            if (_pools.TryGetValue(key, out var q) && q.Count > 0)
            {
                var go = q.Dequeue();
                if (go) // Destroy됐을 수 있으니 체크
                {
                    go.transform.SetParent(parent, false);
                    go.SetActive(true);
                    return go;
                }
            }

            // 2) 프리팹 핸들이 없고, 현재 로드 중인 게 있으면 합류
            if (!_prefabHandles.ContainsKey(key) && _inflight.TryGetValue(key, out var inflightTask))
            {
                var prefab = await inflightTask;
                var ins = Instantiate(prefab);
                return Activate(key, ins, parent);
            }

            // 3) 프리팹 핸들이 없으면 로드
            if (!_prefabHandles.ContainsKey(key))
            {
                var tcs = LoadPrefabInternal(key);
                _inflight[key] = tcs;
                var prefab = await tcs;
                _inflight.Remove(key);
                if (prefab == null) return null;
            }

            // 4) 인스턴스 생성
            var instance = Instantiate(_prefabHandles[key].Result);
            return Activate(key, instance, parent);
        }

        // 비활성화 
        public void Hide(string key, GameObject go)
        { 
            if (!go) return;
            go.SetActive(false);
            if (_active.TryGetValue(key, out var activeGo) && activeGo == go)
                _active.Remove(key);
            if (!_pools.TryGetValue(key, out var q))
            {
                q = new Queue<GameObject>();
                _pools[key] = q;
            }
            q.Enqueue(go);
        }
        public void Unload(string key)
        {
            if (_pools.TryGetValue(key, out var q))
            {
                while (q.Count > 0)
                {
                    var go = q.Dequeue();
                    if (go) Destroy(go);
                }
                _pools.Remove(key);
            }
            if (_active.TryGetValue(key, out var activeGo) && activeGo)
            {
                Destroy(activeGo);
                _active.Remove(key);
            }

            if (_prefabHandles.TryGetValue(key, out var handle))
            {
                if (handle.IsValid()) Addressables.Release(handle);
                _prefabHandles.Remove(key);
            }
        }
        public void ClearAll()
        {
            foreach (var kv in _pools)
            {
                var q = kv.Value;
                while (q.Count > 0)
                {
                    var go = q.Dequeue();
                    if (go) Destroy(go);
                }
            }
            _pools.Clear();
            foreach (var kv in _active)
            {
                if (kv.Value) Destroy(kv.Value);
            }
            _active.Clear();

            foreach (var kv in _prefabHandles)
            {
                if (kv.Value.IsValid()) Addressables.Release(kv.Value);
            }
            _prefabHandles.Clear();

            _inflight.Clear();
        }

        private async Task<GameObject> LoadPrefabInternal(string key)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(key);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[UIPrefabPool] Failed to load prefab: {key}");
                return null;
            }

            _prefabHandles[key] = handle;
            if (!_pools.ContainsKey(key)) _pools[key] = new Queue<GameObject>();
            return handle.Result;
        }
        private GameObject Activate(string key, GameObject instance, Transform parent)
        {
            if (!instance) return null;
            instance.transform.SetParent(parent, false);
            instance.SetActive(true);

            _active[key] = instance;  
            return instance;
        }
    }

}