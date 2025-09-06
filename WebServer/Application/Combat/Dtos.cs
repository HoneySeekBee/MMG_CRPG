using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public sealed record CombatLogEventDto(
        int TMs, string Type, string Actor, string? Target, int? Damage, bool? Crit,
        IReadOnlyDictionary<string, object?>? Extra);

    // 로그 페이징
    public sealed record CombatLogPageDto(
        long CombatId,
        IReadOnlyList<CombatLogEventDto> Items,
        string? NextCursor
    );
    public sealed record CombatLogSummaryDto(
        long CombatId, int TotalEvents, int DurationMs, int DamageDone, int DamageTaken /* etc */);
    public sealed record StageMasterDto(long StageId, IReadOnlyList<long> EnemyCharacterIds /* ... */);
    public sealed record CharacterMasterDto(long CharacterId, int BaseHp, int BaseAtk, int BaseDef, int BaseAspd /* ... */);
    public sealed record SkillMasterDto(long SkillId, int CooldownMs, float Coeff /* ... */);
}
