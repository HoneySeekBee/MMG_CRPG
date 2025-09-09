using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Stages
{
    public sealed record StageListFilter(
        int? Chapter,
        bool? IsActive,
        string? Search,
        int Page = 1,
        int PageSize = 20
    );

    // ─────────────────────────────────────────────
    // 공통 하위 구조 (그래프 입력용)
    // ─────────────────────────────────────────────
    public sealed record EnemyCmd(
        int EnemyCharacterId,
        short Level,
        short Slot,
        string? AiProfile
    );

    public sealed record WaveCmd(
        short Index,
        IReadOnlyList<EnemyCmd> Enemies
    );

    public sealed record DropCmd(
        int ItemId,
        decimal Rate,          // 0..1
        short MinQty,
        short MaxQty,
        bool FirstClearOnly
    );

    public sealed record RewardCmd(
        int ItemId,
        short Qty
    );

    public sealed record RequirementCmd(
        int? RequiredStageId,
        short? MinAccountLevel
    );

    // ─────────────────────────────────────────────
    // Create / Update (Commands)
    // ─────────────────────────────────────────────
    public sealed record CreateStageRequest(
        int Chapter,
        int Order,
        short RecommendedPower,
        short StaminaCost,
        bool IsActive,
        string? Name,                               // 테이블에 Name 추가하기로 했음 (varchar(50/64))
        IReadOnlyList<WaveCmd> Waves,
        IReadOnlyList<DropCmd> Drops,
        IReadOnlyList<RewardCmd> FirstRewards,
        IReadOnlyList<RequirementCmd> Requirements
    );

    public sealed record UpdateStageRequest(
        int Id,
        int Chapter,
        int Order,
        short RecommendedPower,
        short StaminaCost,
        bool IsActive,
        string? Name,
        IReadOnlyList<WaveCmd> Waves,
        IReadOnlyList<DropCmd> Drops,
        IReadOnlyList<RewardCmd> FirstRewards,
        IReadOnlyList<RequirementCmd> Requirements
    );

    // ─────────────────────────────────────────────
    // 전투 연동(Battle) 요청/응답 (계획표 2.4 반영)
    // ─────────────────────────────────────────────
    public sealed record StartBattleRequest(
        int StageId,
        IReadOnlyList<int> Team            // 캐릭터/유닛 식별자
    );

    public sealed record StartBattleResponse(
        Guid BattleId,                     // 서버 발급
        int StaminaAfter
    );

    public sealed record FinishBattleRequest(
        Guid BattleId,
        bool Success,
        int Turns,
        short Stars                        // 0~3 등 규칙
    );

    public sealed record DropResultDto(int ItemId, short Quantity);

    public sealed record FinishBattleResponse(
        IReadOnlyList<DropResultDto> Drops,
        int Exp,
        int Gold,
        bool FirstClear
    );
}
