using Game.Core;
using Game.Network;
using Game.UICommon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public IEnumerator CoLoadCharacterCache(ProtoHttpClient http, Popup popup, float timeoutSeconds = 0f)
    {
        // 완료 플래그
        bool expDone = false;
        bool charDone = false;

        // (선택) 실패 여부를 따로 보고 싶다면 이 플래그 사용
        bool expFailed = false;
        bool charFailed = false;

        // 래퍼 코루틴: target이 끝나면 onDone 호출
        IEnumerator Wrap(IEnumerator target, System.Action onDone, System.Action? onFail = null)
        { 
            yield return target;
            onDone?.Invoke();
        }

        // 동시에 시작
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
}
