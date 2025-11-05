using Contracts.Protos;
using Game.Core;
using Game.Network;
using Game.UICommon;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Net.WebRequestMethods;

public class BattleContentsCache : MonoBehaviour
{
    public static BattleContentsCache Instance { get; private set; }

    // 받아서 보관해둘 것들
    private readonly Dictionary<int, BattlePb> _battles = new();
    private readonly Dictionary<int, ChapterPb> _chapters = new();
    private readonly Dictionary<int, StagePb> _stages = new();

    private readonly Dictionary<int, int> _stageToBattleType = new(); 
    private readonly Dictionary<int, int> _stageToChapter = new();

    public IReadOnlyDictionary<int, BattlePb> Battles => _battles;
    public IReadOnlyDictionary<int, ChapterPb> Chapters => _chapters;
    public IReadOnlyDictionary<int, StagePb> Stages => _stages;

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
     
    public IEnumerator CoLoadContents(ProtoHttpClient Http, Popup popup)
    { 
        // 1) battles
        yield return Http.Get(ApiRoutes.BattlesProto, BattlesPb.Parser, (ApiResult<BattlesPb> res) =>
        { 
            if (!res.Ok || res.Data == null)
            {
                Debug.LogError($"[BattleContentsCache] Battles 실패: {res.Message}");
                popup?.Show($"Battles 불러오기 실패: {res.Message}");
                return;
            }

            _battles.Clear();
            foreach (var b in res.Data.Battles)
                _battles[b.Id] = b; 
        });

        // 2) chapters
        yield return Http.Get(ApiRoutes.ChaptersProto, ChaptersPb.Parser, (ApiResult<ChaptersPb> res) =>
        { 
            if (!res.Ok || res.Data == null)
            {
                Debug.LogError($"[BattleContentsCache] Chapters 실패: {res.Message}");
                popup?.Show($"Chapters 불러오기 실패: {res.Message}");
                return;
            }

            _chapters.Clear();
            foreach (var c in res.Data.Chapters)
                _chapters[c.ChapterId] = c; 
        });

        // 3) stages
        yield return Http.Get(ApiRoutes.StagesProto, StageListPb.Parser, (ApiResult<StageListPb> res) =>
        { 
            if (!res.Ok || res.Data == null)
            {
                Debug.LogError($"[BattleContentsCache] Stages 실패: {res.Message}");
                popup?.Show($"Stages 불러오기 실패: {res.Message}");
                return;
            }

            _stages.Clear();
            foreach (var s in res.Data.Stages)
                _stages[s.Id] = s; 
        });

        BuildLookupMaps();
    }
    public void BuildLookupMaps()
    {
        _stageToBattleType.Clear();
        _stageToChapter.Clear();

        foreach (var stage in _stages.Values)
        {
            // 1) stage → chapter
            var chapterId = stage.Chapter;   
            _stageToChapter[stage.Id] = chapterId;

            // 2) chapter → battleType
            if (_chapters.TryGetValue(chapterId, out var chapter))
            {
                int battleType = chapter.BattleId;  
                _stageToBattleType[stage.Id] = battleType;
            }
        }
    }
    // 편의 메서드들
    public bool TryGetBattle(int id, out BattlePb b) => _battles.TryGetValue(id, out b);
    public bool TryGetChapter(int id, out ChapterPb c) => _chapters.TryGetValue(id, out c);
    public bool TryGetStage(int id, out StagePb s) => _stages.TryGetValue(id, out s); 
    
    public StagePb? GetStage(int stageId)
        => _stages.TryGetValue(stageId, out var s) ? s : null;

    public List<StagePb> GetStagesByChapter(int chapterId)
        => _stages.Values.Where(s => s.Chapter == chapterId).OrderBy(s => s.Order).ToList();

    public List<ChapterPb> GetAllChapters() => _chapters.Values.ToList();
    public int? GetBattleTypeByStage(int stageId)
    {
        return _stageToBattleType.TryGetValue(stageId, out var bt) ? bt : (int?)null;
    }

    public int? GetChapterByStage(int stageId)
    {
        return _stageToChapter.TryGetValue(stageId, out var ch) ? ch : (int?)null;
    }
}
