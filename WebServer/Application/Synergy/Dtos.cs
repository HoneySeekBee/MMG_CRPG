using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Synergy
{
    public sealed record SynergyDto(
        int SynergyId, string Key, string Name, string Description,
        int? IconId, JsonDocument Effect, short Stacking,
        bool IsActive, DateTime? StartAt, DateTime? EndAt,
        IReadOnlyList<SynergyBonusDto> Bonuses,
        IReadOnlyList<SynergyRuleDto> Rules
    );
    public sealed record SynergyBonusDto(int SynergyId, short Threshold, JsonDocument Effect, string? Note);
    public sealed record SynergyRuleDto(int SynergyId, short Scope, short Metric, int RefId, int RequiredCnt, JsonDocument? Extra);


}
