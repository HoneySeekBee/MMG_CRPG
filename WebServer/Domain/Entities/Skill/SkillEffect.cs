using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Skill
{
    public sealed record SkillEffect
    {
        public DamageEffect? Damage { get; init; }
        public HealEffect? Heal { get; init; }
        public BuffEffect? Buff { get; init; }
        public DebuffEffect? Debuff { get; init; }
        public PassiveEffect? Passive { get; init; }
        public TargetingEffect? Targeting { get; init; }
    }
}
