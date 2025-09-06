using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Events
{
    public sealed record CombatLogEvent(
        int TMs,                       // 이벤트 시각(ms)
        string Type,                   // "basic" | "skill" | "death" | "buff" ...
        string Actor,                  // 예: "ally:1"
        string? Target,                // 예: "enemy:2" (없을 수 있음)
        int? Damage,                   // 데미지(없을 수 있음)
        bool? Crit,                    // 치명타 여부(없을 수 있음)
        IReadOnlyDictionary<string, object?>? Extra // 부가정보: 효과/대상배열/힐량 등
    )
    {
        public static CombatLogEvent BasicHit(int tMs, string actor, string target, int dmg, bool crit)
            => new(tMs, CombatLogTypes.Basic, actor, target, dmg, crit, null);

        public static CombatLogEvent Skill(int tMs, string actor, string? target, IReadOnlyDictionary<string, object?> extra)
            => new(tMs, CombatLogTypes.Skill, actor, target, null, null, extra);

        public static CombatLogEvent Death(int tMs, string unitRef)
            => new(tMs, CombatLogTypes.Death, unitRef, null, null, null, null);

        public static CombatLogEvent BuffApplied(int tMs, string actor, string target, string buffName, int durationMs)
            => new(tMs, CombatLogTypes.Buff, actor, target, null, null,
                   new Dictionary<string, object?> { ["name"] = buffName, ["durationMs"] = durationMs });
    }
    public static class CombatLogTypes
    {
        public const string Basic = "basic";
        public const string Skill = "skill";
        public const string Death = "death";
        public const string Buff = "buff";
        public const string Debuff = "debuff";
        public const string Heal = "heal";
        public const string Stun = "stun";
        // 필요해지면 계속 추가
    }
    public sealed record CombatLogPage(
        long CombatId,
        IReadOnlyList<CombatLogEvent> Items,
        string? NextCursor // (예) "t:5000#row:200" 등
    );
}
