using Game.Core;
using Game.Network;
using Game.UICommon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebServer.Protos.Monsters;

public class MonsterCache : MonoBehaviour
{
    public static MonsterCache Instance { get; private set; }

    [Header("Monster")]
    public long MonsterVersion = 0;    
    
    // 본체: id -> MonsterPb
    public Dictionary<int, MonsterPb> MonstersById = new();

    // 인덱스
    public Dictionary<int, List<int>> IdsByElement = new();                
    public Dictionary<string, List<int>> IdsByModelKey = new(StringComparer.OrdinalIgnoreCase);  

    // 스탯: monsterId -> (level -> stat)
    public Dictionary<int, Dictionary<int, MonsterStatPb>> StatByMonsterAndLevel = new();

    // 원본 응답(필요시 디버그/검증용)
    private MonsterListResponsePb _resp;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Clear()
    {
        MonsterVersion = 0;
        MonstersById.Clear();
        IdsByElement.Clear();
        IdsByModelKey.Clear();
        StatByMonsterAndLevel.Clear();
        _resp = null;
    }
    public IEnumerator CoLoadMonsterCache(ProtoHttpClient http, Popup popup, float timeoutSeconds = 0f)
    {
        Clear();

        bool done = false;
        bool ok = false;

        yield return http.Get(
            ApiRoutes.Monsters,                       // 서버 라우트 상수 (예: "/proto/monsters")
            MonsterListResponsePb.Parser,
            (ApiResult<MonsterListResponsePb> res) =>
            {
                ok = res.Ok && res.Data != null;
                if (!ok)
                {
                    popup?.Show("몬스터 불러오기 실패");
                    Debug.LogError($"[FAILED] Load Monster Cache: {res.Message}");
                }
                else
                {
                    _resp = res.Data;
                }
                done = true;
            });

        // 타임아웃 옵션
        if (!done && timeoutSeconds > 0f)
        {
            float start = Time.time;
            while (!done && Time.time - start < timeoutSeconds) yield return null;
            if (!done) { Debug.LogWarning("[MonsterCache] load timeout"); yield break; }
        }

        if (!ok || _resp == null || _resp.Monsters.Count == 0) yield break;

        MonsterVersion = _resp.Version;

        // 용량 미리
        int n = _resp.Monsters.Count;
        MonstersById = new Dictionary<int, MonsterPb>(n);
        StatByMonsterAndLevel = new Dictionary<int, Dictionary<int, MonsterStatPb>>(n);

        foreach (var m in _resp.Monsters)
        {
            MonstersById[m.Id] = m;

            // 인덱스: element
            int? elementId = m.ElementId != null ? m.ElementId.Value : (int?)null;
            if (elementId.HasValue) AddToMultiMap(IdsByElement, elementId.Value, m.Id);

            // 인덱스: modelKey
            if (!string.IsNullOrWhiteSpace(m.ModelKey))
                AddToMultiMap(IdsByModelKey, m.ModelKey, m.Id);

            // 스탯 인덱스: level -> MonsterStatPb
            if (m.Stats != null && m.Stats.Count > 0)
            {
                var byLevel = new Dictionary<int, MonsterStatPb>(m.Stats.Count);
                foreach (var s in m.Stats)
                    byLevel[s.Level] = s;
                StatByMonsterAndLevel[m.Id] = byLevel;
            }
        }

        SortIndexLists(IdsByElement);
        SortIndexLists(IdsByModelKey);

        Debug.Log($"[MonsterCache] 몬스터 {_resp.Monsters.Count}개 로드 (ver={MonsterVersion})");
    }


    public bool Contains(int monsterId) => MonstersById.ContainsKey(monsterId);
    public MonsterPb Get(int monsterId)
        => MonstersById.TryGetValue(monsterId, out var m) ? m : null;
    public IReadOnlyList<int> GetByElement(int elementId)
        => IdsByElement.TryGetValue(elementId, out var list) ? list : Array.Empty<int>();
    public IReadOnlyList<int> GetByModelKey(string modelKey)
        => IdsByModelKey.TryGetValue(modelKey, out var list) ? list : Array.Empty<int>();
    public bool TryGetStat(int monsterId, int level, out MonsterStatPb stat)
    {
        stat = null;
        return StatByMonsterAndLevel.TryGetValue(monsterId, out var byLv) &&
               byLv.TryGetValue(level, out stat);
    }
    public int GetMaxLevel(int monsterId)
    {
        if (!StatByMonsterAndLevel.TryGetValue(monsterId, out var byLv) || byLv.Count == 0)
            return 0;
        int max = 0;
        foreach (var lv in byLv.Keys) if (lv > max) max = lv;
        return max;
    }
    private static void AddToMultiMap<TKey>(Dictionary<TKey, List<int>> map, TKey key, int id)
    {
        if (!map.TryGetValue(key, out var list)) { list = new List<int>(); map[key] = list; }
        list.Add(id);
    }
    private static void SortIndexLists<TKey>(Dictionary<TKey, List<int>> map)
    {
        foreach (var kv in map) kv.Value.Sort();
    }
}
