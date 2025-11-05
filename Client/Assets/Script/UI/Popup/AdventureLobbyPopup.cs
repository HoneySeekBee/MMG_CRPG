using Contracts.Protos;
using Game.Data;
using Lobby;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdventureLobbyPopup : UIPopup
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown ChapterTitles;
    [SerializeField] private Transform Row1;
    [SerializeField] private Transform Row2;
    [SerializeField] private StageButtonPopup prefab;

    private int _currentBattleId = 1;
    private List<ChapterPb> _currentChapterList = new();
    private ChapterPb _currentChapter;

    // 오브젝트 풀링으로 바꾸기
    private readonly List<StageButtonPopup> _pool = new();
    private readonly List<StageButtonPopup> _activeButtons = new();

    public void Set()
    {
        var cache = BattleContentsCache.Instance;
        var user = GameState.Instance.CurrentUser;
        var progMgr = user.StageProgress;

        var chapters = cache.Chapters
       .Values
       .Where(c => c.BattleId == _currentBattleId && c.IsActive)    
       .OrderBy(c => c.ChapterNum)
       .ToList();

        var cleared = new HashSet<int>();
        foreach (var p in progMgr.GetAll)
        {
            if (p.Cleared)
                cleared.Add(p.StageId);
        }

        StagePb nextStage = null;
        ChapterPb nextChapter = null;

        foreach (var chapter in chapters)
        {
            // 이 챕터에 속한 스테이지들
            var stages = cache
                .GetStagesByChapter(chapter.ChapterId)
                .Where(s => s.IsActive)          
                .OrderBy(s => s.Order)              
                .ToList();

            foreach (var stage in stages)
            {
                if (!cleared.Contains(stage.Id))
                {
                    nextStage = stage;
                    nextChapter = chapter;
                    break;
                }
            }
            if (nextStage != null)
                break;
        }
        if (nextStage == null)
        {
            nextChapter = chapters.LastOrDefault();
        }
        _currentChapter = nextChapter;
        _currentChapterList = chapters; 
        Debug.Log($"[AdventureLobbyPopup] 다음에 플레이할 스테이지  " +
                  $"Battle={_currentBattleId}, Chapter={nextChapter.ChapterNum}({nextChapter.Name}), " +
                  $"Stage={nextStage.Order}({nextStage.Name})");
        PopulateChapterDropdown(chapters, _currentChapter);

        if (_currentChapter != null)
            RenderStagesForChapter(_currentChapter.ChapterId);
    }
    private void PopulateChapterDropdown(List<ChapterPb> allChapters, ChapterPb currentChapter)
    {
        ChapterTitles.onValueChanged.RemoveAllListeners();
        ChapterTitles.ClearOptions(); 

        var options = new List<TMP_Dropdown.OptionData>();

        // 현재 챕터 이하인 애들 보여주기 
        int maxChapterNum = currentChapter != null ? currentChapter.ChapterNum : 1;

        foreach (var ch in allChapters)
        {
            if (ch.ChapterNum > maxChapterNum)
                break; 

            string label = $"챕터 {ch.ChapterNum} - {ch.Name}";
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        ChapterTitles.AddOptions(options);

        // 현재 챕터로 선택 맞춰주기
        if (currentChapter != null)
        {
            int index = currentChapter.ChapterNum - 1; // 0-based
            if (index >= 0 && index < ChapterTitles.options.Count)
            {
                ChapterTitles.value = index;
                ChapterTitles.RefreshShownValue();  
            } 
        }  
        ChapterTitles.onValueChanged.AddListener(OnChapterChanged);
    }

    private void OnChapterChanged(int idx)
    {
        if (_currentChapterList != null && idx >= 0 && idx < _currentChapterList.Count)
        {
            var ch = _currentChapterList[idx];
            _currentChapter = ch;
            RenderStagesForChapter(ch.ChapterId);
        }
    }
    private void RenderStagesForChapter(int chapterId)
    {
        ReturnAllButtonsToPool();
         

        var cache = BattleContentsCache.Instance;
        var user = GameState.Instance.CurrentUser;
        var prog = user.StageProgress;   

        // 이 챕터의 스테이지들
        var stages = cache.GetStagesByChapter(chapterId)
                          .Where(s => s.IsActive)
                          .OrderBy(s => s.Order)
                          .ToList();

        // 1) 클리어한 건 전부 열림
        // 2) 아직 안 깬 것 중 첫 번째만 열림
        var cleared = new HashSet<int>();
        foreach (var p in prog.GetAll)
        {
            if (p.Cleared)
                cleared.Add(p.StageId);
        }

        // 아직 안 깬 것 중 첫 번째 stageId 찾기
        int? firstLockedStageId = null;
        foreach (var s in stages)
        {
            if (!cleared.Contains(s.Id))
            {
                firstLockedStageId = s.Id;
                break;
            }
        }

        foreach (var s in stages)
        {
            // 홀/짝에 따라 부모 결정
            Transform parent = (s.Order % 2 == 1) ? Row1 : Row2;

            var btn = GetButtonFromPool(parent);

            bool isActive = false;
            int stars = 0;

            if (cleared.Contains(s.Id))
            {
                isActive = true;
                stars = prog.GetStars(s.Id); 
            }
            else
            {
                // 아직 안 깬 것 중 첫 번째만 열어주기
                isActive = (firstLockedStageId.HasValue && firstLockedStageId.Value == s.Id);
            }

            btn.Set(
                chapterNum: _currentChapter.ChapterNum,
                stageNum: s.Order,
                onStageClicked: () =>
                {
                    Debug.Log($"[AdventureLobbyPopup] 스테이지 클릭: {s.Id} ({_currentChapter.ChapterNum}-{s.Order})");
                    // 여기서 실제 입장 로직 호출
                },
                isActive: isActive,
                score: stars
            );
        }
    }
    private StageButtonPopup GetButtonFromPool(Transform parent)
    {
        StageButtonPopup item = null;

        if (_pool.Count > 0)
        {
            item = _pool[_pool.Count - 1];
            _pool.RemoveAt(_pool.Count - 1);
        }
        else
        {
            item = Instantiate(prefab);
        }

        item.transform.SetParent(parent, false);
        item.gameObject.SetActive(true);

        _activeButtons.Add(item);

        return item;
    }
    private void ReturnAllButtonsToPool()
    {
        for (int i = 0; i < _activeButtons.Count; i++)
        {
            var btn = _activeButtons[i];
            btn.gameObject.SetActive(false);
            // 필요하면 공용 부모로 옮겨도 되고, 그냥 두어도 됨
            _pool.Add(btn);
        }
        _activeButtons.Clear();
    }
    private void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            Destroy(t.GetChild(i).gameObject);
        }
    }
}
