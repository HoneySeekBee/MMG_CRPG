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
    }
}
