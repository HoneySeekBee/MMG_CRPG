using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Skill
{
    public sealed record DebuffEffect
    {
        public int SkillId { get; init; }

        public int? BleedDamage { get; init; }
        public int? BleedDuration { get; init; }

        public float? DelayedMultiplier { get; init; }
        public float? DelayedDelaySec { get; init; }

        public int? StunDurationMs { get; init; }
        public int? SilenceDurationMs { get; init; }
        public int? FreezeDurationMs { get; init; }
        public int? RootDurationMs { get; init; }
        public int? KnockdownDurationMs { get; init; }
        public float? KnockbackDistance { get; init; }
        public float? KnockbackSpeed { get; init; }
        public float? PullDistance { get; init; }
        public float? PullSpeed { get; init; }
        public float? PushDistance { get; init; }
        public float? PushSpeed { get; init; }
        public float? PushDirX { get; init; }
        public float? PushDirZ { get; init; }

    }
}
