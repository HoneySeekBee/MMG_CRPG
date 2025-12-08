using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Runtime
{
    public class AppliedBuff
    {
        public BuffKind Kind { get; init; }
        public int SkillId { get; init; }
        public float Value { get; init; }
        public int DurationMs { get; set; }
        public int MaxDurationMs { get; init; }
        public int Stacks { get; set; } = 1;

        public bool IsExpired => DurationMs <= 0;

        public bool IsDebuff =>
            Kind == BuffKind.Bleed ||
            Kind == BuffKind.Burn ||
            Kind == BuffKind.Poison ||
            Kind == BuffKind.Stun ||
            Kind == BuffKind.Silence;
    }
}
