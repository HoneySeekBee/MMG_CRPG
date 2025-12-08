using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Skill
{
    public sealed record DamageEffect
    {
        public int SkillId { get; init; }
        public int Hits { get; init; } = 1;
        public float Multiplier { get; init; } = 1f;
        public float? HpRatioFactor { get; init; }
    }
}
