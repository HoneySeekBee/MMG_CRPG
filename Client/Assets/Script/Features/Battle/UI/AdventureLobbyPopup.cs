using Contracts.Protos;
using Game.Data;
using Game.Logging;
using Lobby;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    // ������Ʈ Ǯ������ �ٲٱ�
    private readonly List<StageButtonPopup> _pool = new();
    private readonly List<StageButtonPopup> _activeButtons = new();

    [SerializeField] private Button beforeBtn;

    public void Set()
    { 
        UserPartyNetwork partyNetwork = NetworkManager.Instance.PartyNetwork;
        StartCoroutine(partyNetwork.GetPartyAsync(NetworkManager.BATTLE_ADVENTURE));
        beforeBtn.onClick.RemoveAllListeners();
        beforeBtn.onClick.AddListener(() => LobbyRootController.Instance.Show("Battle"));
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
            // �� é�Ϳ� ���� ����������
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
        GameLogger.Info($"[AdventureLobbyPopup]  " +
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
         
        int maxChapterNum = currentChapter != null ? currentChapter.ChapterNum : 1;

        foreach (var ch in allChapters)
        {
            if (ch.ChapterNum > maxChapterNum)
                break; 

            string label = $"é�� {ch.ChapterNum} - {ch.Name}";
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        ChapterTitles.AddOptions(options);

        // ���� é�ͷ� ���� �����ֱ�
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

        // �� é���� ����������
        var stages = cache.GetStagesByChapter(chapterId)
                          .Where(s => s.IsActive)
                          .OrderBy(s => s.Order)
                          .ToList();

        // 1) Ŭ������ �� ���� ����
        // 2) ���� �� �� �� �� ù ��°�� ����
        var cleared = new HashSet<int>();
        foreach (var p in prog.GetAll)
        {
            if (p.Cleared)
                cleared.Add(p.StageId);
        }

        // ���� �� �� �� �� ù ��° stageId ã��
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
            // Ȧ/¦�� ���� �θ� ����
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
                // ���� �� �� �� �� ù ��°�� �����ֱ�
                isActive = (firstLockedStageId.HasValue && firstLockedStageId.Value == s.Id);
            }

            btn.Set(
                chapterNum: _currentChapter.ChapterNum,
                stageNum: s.Order,
                onStageClicked: () =>
                {
                    GameLogger.Info($"[AdventureLobbyPopup]: {s.Id} ({_currentChapter.ChapterNum}-{s.Order})"); 
                    if(isActive)
                        _ = OpenStageDetailPopup(s);
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
            btn.transform.SetParent(this.transform, false);
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
    private async Task OpenStageDetailPopup(StagePb data)
    {
        try
        {
            const string key = "StageDetailUI";

            var popupPool = UIPrefabPool.Instance as UIPopupPool
                            ?? FindObjectOfType<UIPopupPool>();
            if (popupPool == null) { GameLogger.Error("UIPopupPool not found"); return; }

            var popup = await popupPool.ShowPopupAsync<AdventureDetailPopup>(key, this.transform);
            if (popup == null) { GameLogger.Error("AdventureDetailPopup open failed"); return; }
            popup.Set(data);
        }
        catch (Exception e)
        {
            GameLogger.Error($"[AdventureLobbyPopup] OpenStageDetailPopup failed: {e.Message}");
        }
    }
}
