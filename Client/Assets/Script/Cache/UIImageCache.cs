using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class UIImageCache : MonoBehaviour
{
    public static UIImageCache Instance { get; private set; }

    [SerializeField] private string uiSpriteLabel = "UISprite";
    public const string YellowStarKey = "StarYellow";
    public const string GrayStarKey = "StarGray";
    public const string ButtonGreenKey = "Green";
    public const string ButtonGrayKey = "Gray";

    private readonly Dictionary<string, AsyncOperationHandle<Sprite>> _cache = new();
    private AsyncOperationHandle<IList<IResourceLocation>> _locationsHandle;
    private bool _preloaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator PreloadAllUISprites()
    {
        if (_preloaded) yield break;

        // 1) 라벨에 해당하는 Sprite 로케이션 조회
        _locationsHandle = Addressables.LoadResourceLocationsAsync(uiSpriteLabel, typeof(Sprite));
        yield return _locationsHandle;

        if (!_locationsHandle.IsValid() || _locationsHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[UIImageCache] Failed to get locations for label: {uiSpriteLabel}");
            yield break;
        }

        // 2) 각 로케이션(=개별 Addressable 항목) 로드 & 캐시
        foreach (var loc in _locationsHandle.Result)
        {
            // PrimaryKey가 런타임에서 사용할 수 있는 고유 키(그룹명과 무관)
            string key = loc.PrimaryKey;

            // 이미 로드된 키는 스킵
            if (_cache.ContainsKey(key)) continue;

            var handle = Addressables.LoadAssetAsync<Sprite>(loc);
            _cache[key] = handle;
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[UIImageCache] Failed to load sprite: {key}");
                _cache.Remove(key);
            }
        }

        _preloaded = true;
        Debug.Log($"[UIImageCache] Preloaded {_cache.Count} sprites with label '{uiSpriteLabel}'.");
    }
    public Sprite Get(string key)
    {
        if (_cache.TryGetValue(key, out var handle) && handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;
        return null;
    }
    public bool TryGet(string key, out Sprite sprite)
    {
        sprite = null;
        if (_cache.TryGetValue(key, out var handle) && handle.Status == AsyncOperationStatus.Succeeded)
        {
            sprite = handle.Result;
            return true;
        }
        return false;
    }
}
