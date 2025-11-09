using Contracts.CharacterModel;
using Game.Core;
using Game.Network;
using Game.UICommon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using WebServer.Protos;
public sealed class ExpCurve
{
    public int RarityId { get; }
    public int MaxLevel { get; }
    public int[] RequiredExpByLevel { get; } // index == level

    public ExpCurve(int rarityId, int maxLevel)
    {
        RarityId = rarityId;
        MaxLevel = maxLevel;
        RequiredExpByLevel = new int[maxLevel + 1]; // 0은 비움, 1..MaxLevel 사용
    }

    public int GetRequiredExp(int level)
        => (level >= 0 && level < RequiredExpByLevel.Length) ? RequiredExpByLevel[level] : 0;
}
public class CharacterCache : MonoBehaviour
{
    public static CharacterCache Instance { get; private set; }


    [Header("CharacterEXP")]
    public long ExpVersion  = 0;
    public Dictionary<int, ExpCurve> CharacterExpDict = new();
    private CharacterExpTableResponse _characterExps;

    [Header("Character")]
    public long CharacterVersion = 0;
    public Dictionary<int, CharacterSummaryPb> SummaryById = new();
    public Dictionary<int, CharacterDetailPb> DetailById = new();
     
    public Dictionary<int, List<int>> IdsByRarity = new();
    public Dictionary<int, List<int>> IdsByElement = new();
    public Dictionary<int, List<int>> IdsByRole = new();
    public Dictionary<int, List<int>> IdsByFaction = new();

    public Dictionary<string, List<int>> IdsByTag = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<int, List<int>> CharacterIdsBySkillId = new();

    private CharactersResponsePb _characters;

    [Header("CharacterModel")]
    public List<CharacterModelPb> CharacterModels = new();
    public Dictionary<int, CharacterModelPb> CharacterModelById = new();
    private ListCharacterModelsResponsePb _characterModels;

    public List<CharacterModelPartPb> ModelParts = new();
    public Dictionary<int, CharacterModelPartPb> ModelPartsById = new();
    private ListCharacterModelPartsResponsePb _modelParts;


    public List<CharacterModelWeaponPb> WeaponParts = new();
    public Dictionary<int, CharacterModelWeaponPb> WeaponPartsById = new();
    private ListCharacterModelWeaponsResponsePb _weaponParts;

    [Header("CharacterMesh")]
    public Dictionary<string, AsyncOperationHandle<Mesh>> CharacterMeshByKey = new();
    public Dictionary<string, AsyncOperationHandle<Mesh>> WeaponMeshByKey = new();

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
    private void Init_Model()
    {
        CharacterModels.Clear();
        CharacterModelById.Clear();
        _characterModels = null;

        ModelParts.Clear();
        ModelPartsById.Clear();
        _modelParts = null;
    }
    public IEnumerator CoLoadCharacterModelCache(ProtoHttpClient http, Popup popup, float timeoutSeconds = 0f)
    {
        Init_Model(); // 초기화

        // 요청 바디: 비우면 전체 (서버 컨트롤러 주석 참고)
        var req = new ListCharacterModelsRequestPb();
        bool done = false;

        // POST (proto)
        yield return http.Post(
            ApiRoutes.CharacterModel_List,
            req,
            ListCharacterModelsResponsePb.Parser,
            (ApiResult<ListCharacterModelsResponsePb> res) =>
            {
                if (!res.Ok || res.Data == null)
                {
                    popup?.Show("캐릭터 모델 불러오기 실패");
                    Debug.LogError($"[FAILED] Load CharacterModel Cache {res.Message}");
                    done = true;
                    return;
                }
                _characterModels = res.Data;
                done = true;
            });

        // 타임아웃(optional)
        if (!done && timeoutSeconds > 0f)
        {
            float start = Time.time;
            while (!done && (Time.time - start) < timeoutSeconds)
                yield return null;

            if (!done)
            {
                Debug.LogWarning("[CharacterCache] CharacterModel load timeout");
                yield break;
            }
        }

        if (_characterModels == null || _characterModels.Models.Count == 0)
            yield break;
          
        int n = _characterModels.Models.Count;
        CharacterModels = new List<CharacterModelPb>(n);
        CharacterModelById = new Dictionary<int, CharacterModelPb>(n);

        foreach (var m in _characterModels.Models)
        { 
            CharacterModelById[m.CharacterId] = m;
            CharacterModels.Add(m);
        }
         
        CharacterModels.Sort((a, b) => a.CharacterId.CompareTo(b.CharacterId));

        Debug.Log($"Cache - 캐릭터 모델 {_characterModels.Models.Count}");

        bool partsDone = false;
        yield return http.Get(
            ApiRoutes.CharacterModel_Parts,        // 또는 CharacterModel_Parts 상수
            ListCharacterModelPartsResponsePb.Parser,
            (ApiResult<ListCharacterModelPartsResponsePb> res) =>
            {
                if (!res.Ok || res.Data == null)
                {
                    Debug.LogError("[FAILED] Load CharacterModel Parts");
                    partsDone = true;
                    return;
                }

                _modelParts = res.Data;
                partsDone = true;
            });
        if (_modelParts != null && _modelParts.Parts.Count > 0)
        {
            int cnt = _modelParts.Parts.Count;
            ModelParts = new List<CharacterModelPartPb>(cnt);
            ModelPartsById = new Dictionary<int, CharacterModelPartPb>(cnt);

            foreach (var part in _modelParts.Parts)
            {
                ModelParts.Add(part);
                ModelPartsById[part.PartId] = part;
            }

            Debug.Log($"[CharacterCache] 캐릭터 모델 파츠 {_modelParts.Parts.Count}개 로드");
        }

        bool weaponDone = false;
        yield return http.Get(
           ApiRoutes.CharacterModel_Weapons,        // 또는 CharacterModel_Parts 상수
           ListCharacterModelWeaponsResponsePb.Parser,
           (ApiResult<ListCharacterModelWeaponsResponsePb> res) =>
           {
               if (!res.Ok || res.Data == null)
               {
                   Debug.LogError("[FAILED] Load CharacterModel Parts");
                   partsDone = true;
                   return;
               }

               _weaponParts = res.Data;
               partsDone = true;
           });
        if (_weaponParts != null && _weaponParts.Weapons.Count > 0)
        {
            int cnt = _weaponParts.Weapons.Count;
            WeaponParts = new List<CharacterModelWeaponPb>(cnt);
            WeaponPartsById = new Dictionary<int, CharacterModelWeaponPb>(cnt);

            foreach (var part in _weaponParts.Weapons)
            {
                WeaponParts.Add(part);
                WeaponPartsById[part.WeaponId] = part;
            }

            Debug.Log($"[WeaponCache] 캐릭터 모델 파츠 {_weaponParts.Weapons.Count}개 로드");
        } 
    }
    public IEnumerator CoLoadCharacterCache(ProtoHttpClient http, Popup popup, float timeoutSeconds = 0f)
    {
        // 완료 플래그
        bool expDone = false;
        bool charDone = false;
        bool modelDone = false;

        // (선택) 실패 여부를 따로 보고 싶다면 이 플래그 사용
        bool expFailed = false;
        bool charFailed = false;
        bool modelFailed = false;

        // 래퍼 코루틴: target이 끝나면 onDone 호출
        IEnumerator Wrap(IEnumerator target, System.Action onDone, System.Action? onFail = null)
        { 
            yield return target;
            onDone?.Invoke();
        }

        // 동시에 시작
        StartCoroutine(Wrap(CoLoadCharacterModelCache(http, popup), () => modelDone = true, () => modelFailed = true));
        StartCoroutine(Wrap(CoLoadCharacterExp(http, popup), () => expDone = true, () => expFailed = true));
        StartCoroutine(Wrap(CoLoadCharacter(http, popup), () => charDone = true, () => charFailed = true));


        float start = Time.time;
        while (!(expDone && charDone))
        {
            if (timeoutSeconds > 0f && (Time.time - start) > timeoutSeconds)
            {
                Debug.LogWarning("[CharacterCache] Parallel load timeout");
                break;
            }
            yield return null; // 한 프레임 대기 (동시 진행)
        } 

        if (!expDone) Debug.LogWarning("[CharacterCache] CharacterExp load did not complete.");
        if (!charDone) Debug.LogWarning("[CharacterCache] Character list load did not complete.");
         
    }

    private IEnumerator CoLoadCharacterExp(ProtoHttpClient http, Popup popup)
    {
        CharacterExpDict.Clear();

        yield return http.Get(ApiRoutes.CharacterExp, CharacterExpTableResponse.Parser,
            (ApiResult<CharacterExpTableResponse> res) =>
            {
                if (!res.Ok)
                {
                    popup.Show($"캐릭터 필요 경험치 불러오기 실패");
                    Debug.Log("[FAILED] Load CharacterEXP Cache ");
                    return;
                }
                _characterExps = res.Data;
            });
        if (_characterExps == null || _characterExps.Groups.Count == 0)
            yield break;

        ExpVersion = _characterExps.Version;

        foreach (var group in _characterExps.Groups)
        {
            var maxLevel = 0;
            foreach (var lv in group.Levels)
                if (lv.Level > maxLevel) maxLevel = lv.Level;

            var curve = new ExpCurve(group.RarityId, maxLevel);

            foreach (var lv in group.Levels)
                curve.RequiredExpByLevel[lv.Level] = lv.RequiredExp;

            CharacterExpDict[group.RarityId] = curve;
        }
        Debug.Log($"Cache - 캐릭터 필요 경험치 {_characterExps.Groups.Count}");
    }
    private IEnumerator CoLoadCharacter(ProtoHttpClient http, Popup popup)
    {
        // 초기화
        SummaryById.Clear();
        DetailById.Clear();
        IdsByRarity.Clear();
        IdsByElement.Clear();
        IdsByRole.Clear();
        IdsByFaction.Clear();
        IdsByTag.Clear();
        CharacterIdsBySkillId.Clear();
        _characters = null;

        // 1) 네트워크 요청
        yield return http.Get(ApiRoutes.Character, CharactersResponsePb.Parser, (ApiResult<CharactersResponsePb> res) =>
        {
            if (!res.Ok || res.Data == null)
            {
                popup?.Show("캐릭터 불러오기 실패");
                Debug.LogError("[FAILED] Load Character Cache");
                return;
            }
            _characters = res.Data;
        });

        if (_characters == null || _characters.Characters.Count == 0)
            yield break;

        CharacterVersion = _characters.Version;

        // 2) capacity 미리 확보
        int n = _characters.Characters.Count;
        SummaryById = new Dictionary<int, CharacterSummaryPb>(n);
        DetailById = new Dictionary<int, CharacterDetailPb>(n);

        // 3) 투영/인덱싱
        foreach (var ch in _characters.Characters)
        {
            // a) detail 저장
            DetailById[ch.Id] = ch;

            // b) summary 투영 (서버가 SummaryPb 별도로 주면 그걸 쓰면 됨)
            var summary = new CharacterSummaryPb
            {
                Id = ch.Id,
                Name = ch.Name,
                RarityId = ch.RarityId,
                ElementId = ch.ElementId,
                RoleId = ch.RoleId,
                FactionId = ch.FactionId,
                IsLimited = ch.IsLimited,
                ReleaseDate = ch.ReleaseDate // null 가능
            };
            SummaryById[ch.Id] = summary;

            // c) 그룹 인덱스
            AddToMultiMap(IdsByRarity, ch.RarityId, ch.Id);
            AddToMultiMap(IdsByElement, ch.ElementId, ch.Id);
            AddToMultiMap(IdsByRole, ch.RoleId, ch.Id);
            AddToMultiMap(IdsByFaction, ch.FactionId, ch.Id);

            // d) 태그 인덱스
            if (ch.Tags != null)
            {
                foreach (var tag in ch.Tags)
                {
                    if (string.IsNullOrWhiteSpace(tag)) continue;
                    AddToMultiMap(IdsByTag, tag.Trim(), ch.Id);
                }
            }

            // e) 스킬 역인덱스
            if (ch.Skills != null)
            {
                foreach (var sk in ch.Skills)
                    AddToMultiMap(CharacterIdsBySkillId, sk.SkillId, ch.Id);
            }
        }
        Debug.Log($"Cache - 캐릭터  {_characters.Characters.Count}");
        SortIndexLists(IdsByRarity);
        SortIndexLists(IdsByElement);
        SortIndexLists(IdsByRole);
        SortIndexLists(IdsByFaction);
        foreach (var kv in IdsByTag) kv.Value.Sort();
        foreach (var kv in CharacterIdsBySkillId) kv.Value.Sort();
    }

    // ---------- helpers ----------
    private static void AddToMultiMap<TKey>(Dictionary<TKey, List<int>> map, TKey key, int id)
    {
        if (!map.TryGetValue(key, out var list))
        {
            list = new List<int>();
            map[key] = list;
        }
        list.Add(id);
    }
    public bool TryGetRequiredExp(int rarityId, int level, out int requiredExp)
    {
        requiredExp = 0;
        if (!CharacterExpDict.TryGetValue(rarityId, out var curve))
            return false;
        requiredExp = curve.GetRequiredExp(level);
        return requiredExp > 0;
    }
    private static void SortIndexLists(Dictionary<int, List<int>> map)
    {
        foreach (var kv in map)
            kv.Value.Sort();
    }
    public IEnumerator CoPreloadMeshes()
    {
        // 1) 캐릭터 파츠(mesh)들
        yield return PreloadLabelToDict("model", CharacterMeshByKey);

        // 2) 무기(mesh)들
        yield return PreloadLabelToDict("weapon", WeaponMeshByKey);
    }

    private IEnumerator PreloadLabelToDict(string label, Dictionary<string, AsyncOperationHandle<Mesh>> dict)
    {
        var locHandle = Addressables.LoadResourceLocationsAsync(label, typeof(Mesh));
        yield return locHandle;

        if (locHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[MeshCache] 라벨 '{label}' 로케이션 로드 실패");
            yield break;
        }

        var locations = locHandle.Result;
        Debug.Log($"[MeshCache] {label} 에서 {locations.Count}개 위치 발견");

        foreach (var loc in locations)
        {
            string key = loc.PrimaryKey;

            if (dict.ContainsKey(key))
                continue;

            var meshHandle = Addressables.LoadAssetAsync<Mesh>(loc);
            yield return meshHandle;

            if (meshHandle.Status == AsyncOperationStatus.Succeeded)
            {
                dict[key] = meshHandle;
                Debug.Log($"[MeshCache] ({label}) 캐시: {key}");
            }
            else
            {
                Debug.LogWarning($"[MeshCache] ({label}) {key} 로드 실패");
            }
        }

        // 필요시: Addressables.Release(locHandle);
    }

    // ─── 조회 메서드 ───
    public Mesh GetCharacterMesh(string key)
    {
        if (CharacterMeshByKey.TryGetValue(key, out var handle) &&
            handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;
        return null;
    }

    public Mesh GetWeaponMesh(string key)
    {
        if (WeaponMeshByKey.TryGetValue(key, out var handle) &&
            handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;
        return null;
    }

    private void OnDestroy()
    {
        foreach (var kv in CharacterMeshByKey)
            Addressables.Release(kv.Value);
        CharacterMeshByKey.Clear();

        foreach (var kv in WeaponMeshByKey)
            Addressables.Release(kv.Value);
        WeaponMeshByKey.Clear();
    }
}
