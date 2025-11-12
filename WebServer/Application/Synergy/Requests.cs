using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Synergy
{
    public sealed record CreateSynergyRequest(
        string Key, string Name, string Description, int? IconId,
        JsonDocument Effect, short Stacking,
        bool IsActive = true, DateTime? StartAt = null, DateTime? EndAt = null,
        IReadOnlyList<CreateSynergyBonusRequest>? Bonuses = null,
        IReadOnlyList<CreateSynergyRuleRequest>? Rules = null
    );

    public sealed record UpdateSynergyRequest(
        int SynergyId,
        string? Name = null, string? Description = null, int? IconId = null,
        JsonDocument? Effect = null, short? Stacking = null,
        bool? IsActive = null, DateTime? StartAt = null, DateTime? EndAt = null
    );

    public sealed record CreateSynergyBonusRequest(short Threshold, JsonDocument Effect, string? Note);
    public sealed record CreateSynergyRuleRequest(short Scope, short Metric, int RefId, int RequiredCnt, JsonDocument? Extra);

    // (선택) 평가 요청/응답
    public sealed class EvaluateSynergiesRequest
    {
        // 파티에 포함된 모든 캐릭터의 ElementId/FactionId 목록 (중복 허용)
        // 예: [1,1,2,4] → 불(1) 2명, 물(2) 1명, 바람(4) 1명
        public IReadOnlyList<int> ElementIds { get; init; } = Array.Empty<int>();
        public IReadOnlyList<int> FactionIds { get; init; } = Array.Empty<int>();

        // 캐릭터별 장착 "세트 조각 수" (SetId -> pieceCount)
        public IReadOnlyList<CharacterEquipSummary> Characters { get; init; } = Array.Empty<CharacterEquipSummary>();
    }
    public sealed class CharacterEquipSummary
    {
        public int CharacterId { get; init; }
        public int ElementId { get; init; }
        public int FactionId { get; init; }
        public IReadOnlyDictionary<string, int>? TagCounts { get; init; }
    }
    public sealed record EvaluateResult(string Key, string Name, short? ThresholdReached);
}
