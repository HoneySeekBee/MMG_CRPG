using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Skill
{
    public sealed record HealEffect
    {
        public int SkillId { get; init; }
        public float Multiplier { get; init; }
        public float? PercentMissingHp { get; init; }
    }

}
