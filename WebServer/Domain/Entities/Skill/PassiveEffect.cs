using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Skill
{
    public sealed record PassiveEffect
    {
        public int SkillId { get; init; }
        public float? AtkPerMissingHpPercent { get; init; }
        public float? HealPercent { get; init; }
        public Dictionary<string, float>? PerAllyBonus { get; init; }
        public float? CooldownReduceSec { get; init; }
        public float? ShieldPerAlly { get; init; }
        public float? SelfHpCostPercent { get; init; }
        public int? DefPerUse { get; init; }
        public int? MaxStacks { get; init; }
    }

}
