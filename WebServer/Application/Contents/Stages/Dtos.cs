using Domain.Entities.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Stages
{
    public sealed record StageSummaryDto(
        int Id, int Chapter, int StageNum, string? Name,
        short RecommendedPower, short StaminaCost, bool IsActive);

    // 상세 그래프
    public sealed record EnemyDto(int EnemyCharacterId, short Level, short Slot, string? AiProfile);
    public sealed record WaveDto(short Index, IReadOnlyList<EnemyDto> Enemies);
    public sealed record DropDto(int ItemId, decimal Rate, short MinQty, short MaxQty, bool FirstClearOnly);
    public sealed record RewardDto(int ItemId, short Qty);
    public sealed record RequirementDto(int? RequiredStageId, short? MinAccountLevel);
    public sealed record BatchDto(int BatchNum, string UnitKey, string EnvKey);
    public sealed record StageDetailDto(
        int Id, int Chapter, int Order, string? Name,
        short RecommendedPower, short StaminaCost, bool IsActive,
        IReadOnlyList<WaveDto> Waves,
        IReadOnlyList<DropDto> Drops,
        IReadOnlyList<RewardDto> FirstRewards,
        IReadOnlyList<RequirementDto> Requirements,
        IReadOnlyList<BatchDto> Batches);


    // ─────────────────────────────────────────────
    // Mappings (Domain → DTO)
    // ─────────────────────────────────────────────
    public static class StageMappings
    {
        public static StageSummaryDto ToSummaryDto(this Stage s) =>
            new(s.Id, s.Chapter, s.StageNumber, s.Name, s.RecommendedPower, s.StaminaCost, s.IsActive);

        public static StageDetailDto ToDetailDto(this Stage s) =>
            new(
                s.Id, s.Chapter, s.StageNumber, s.Name, s.RecommendedPower, s.StaminaCost, s.IsActive,
                s.Waves
                 .OrderBy(w => w.Index)
                 .Select(w => new WaveDto(
                     w.Index,
                     w.Enemies
                      .OrderBy(e => e.Slot)
                      .Select(e => new EnemyDto(e.EnemyCharacterId, e.Level, e.Slot, e.AiProfile))
                      .ToList()
                 )).ToList(),
                s.Drops
                 .Select(d => new DropDto(d.ItemId, d.Rate, d.MinQty, d.MaxQty, d.FirstClearOnly))
                 .ToList(),
                s.FirstRewards
                 .Select(r => new RewardDto(r.ItemId, r.Qty))
                 .ToList(),
                s.Requirements
                 .Select(r => new RequirementDto(r.RequiredStageId, r.MinAccountLevel))
                 .ToList(),
                s.Batches
                .Select(r => new BatchDto(r.BatchNum, r.UnitKey, r.EnvKey))
                .ToList()
            );

        // 목록 변환 헬퍼
        public static IReadOnlyList<StageSummaryDto> ToSummaryDtos(this IEnumerable<Stage> stages) =>
            stages.Select(ToSummaryDto).ToList();
    }
}
