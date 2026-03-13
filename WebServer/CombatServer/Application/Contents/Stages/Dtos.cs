using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Stages
{
    public sealed record EnemyDto(int EnemyCharacterId, short Level, short Slot, string? AiProfile);
    public sealed record WaveDto(short Index, IReadOnlyList<EnemyDto> Enemies, int batchNum);
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
}
