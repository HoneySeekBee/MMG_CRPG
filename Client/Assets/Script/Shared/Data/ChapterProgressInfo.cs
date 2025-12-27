using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChapterProgressInfo
{
    public int ChapterId { get; }
    public int ClearedCount { get; }
    public int TotalCount { get; }

    public float ProgressRatio => TotalCount == 0 ? 0f : (float)ClearedCount / TotalCount;

    public ChapterProgressInfo(int chapterId, int clearedCount, int totalCount)
    {
        ChapterId = chapterId;
        ClearedCount = clearedCount;
        TotalCount = totalCount;
    }
}