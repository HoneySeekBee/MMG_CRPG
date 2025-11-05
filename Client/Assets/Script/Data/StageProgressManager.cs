using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageProgressManager
{
    // 전체 진행도 ( stageId, Progress )
    private readonly Dictionary<int, UserStageProgressPb> _stageProgress = new();

    // BattleType 별 진행도 
    private readonly Dictionary<int, Dictionary<int, UserStageProgressPb>> _byBattleType = new();

    // Chapter 별 진행도
    private readonly Dictionary<int, ChapterProgressInfo> _chapterProgress = new();

    public void Sync(MyStageProgressListPb pb)
    {
        _stageProgress.Clear();
        _byBattleType.Clear();
        _chapterProgress.Clear();

        if (pb == null || pb.Progresses.Count == 0)
        {
            Debug.Log("[StageProgressManager] 신규 유저, 진행 데이터 없음");
            return;
        }

        // (1) 기본 딕셔너리에 저장
        foreach (var p in pb.Progresses)
            _stageProgress[p.StageId] = p;

        // (2) BattleType 기준 분류
        foreach (var (stageId, progress) in _stageProgress)
        {
            var battleType = BattleContentsCache.Instance.GetBattleTypeByStage(stageId);
            if (battleType is null) continue;

            if (!_byBattleType.TryGetValue(battleType.Value, out var bucket))
                _byBattleType[battleType.Value] = bucket = new Dictionary<int, UserStageProgressPb>();

            bucket[stageId] = progress;
        }
        // (3) Chapter별 진행도 계산
        foreach (var chapter in BattleContentsCache.Instance.GetAllChapters())
        {
            var chapterStages = BattleContentsCache.Instance.GetStagesByChapter(chapter.ChapterId);
            int total = chapterStages.Count;
            int cleared = 0;
            foreach (var s in chapterStages)
                if (_stageProgress.TryGetValue(s.Id, out var prog) && prog.Cleared)
                    cleared++;

            _chapterProgress[chapter.ChapterId] = new ChapterProgressInfo(chapter.ChapterId, cleared, total);
        }

        Debug.Log($"[StageProgressManager] 총 {_stageProgress.Count}개 스테이지 진행도 동기화 완료");
    }

    public UserStageProgressPb? GetProgress(int stageId)
        => _stageProgress.TryGetValue(stageId, out var p) ? p : null;

    public IReadOnlyDictionary<int, UserStageProgressPb> GetBattleProgress(int battleType)
        => _byBattleType.TryGetValue(battleType, out var dict) ? dict : new Dictionary<int, UserStageProgressPb>();

    public ChapterProgressInfo? GetChapterProgress(int chapterId)
        => _chapterProgress.TryGetValue(chapterId, out var c) ? c : null;

    public void ClearStageProgress()
    {
        _stageProgress.Clear();
    }
    public bool TryGetStageProgress(int stageId, out UserStageProgressPb progress)
        => _stageProgress.TryGetValue(stageId, out progress);

    public int GetStars(int stageId)
        => _stageProgress.TryGetValue(stageId, out var p) ? (int)p.Stars : 0;

    public bool IsStageCleared(int stageId)
        => _stageProgress.TryGetValue(stageId, out var p) && p.Cleared;

    public List<UserStageProgressPb> GetAll => _stageProgress.Select(x => x.Value).ToList();
}