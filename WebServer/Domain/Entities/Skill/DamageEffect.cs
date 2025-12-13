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
         
        public float AtkMultiplier { get; init; } = 0f;   // ATK * x
        public float HpMultiplier { get; init; } = 0f;   // HP  * x
        public float DefMultiplier { get; init; } = 0f;   // DEF * x

        public float Flat { get; init; } = 0f;            // + 10 같은 상수항

        // range 스케일링용
        public bool UsesRange { get; init; } = false;
        public float? MinRange { get; init; }
        public float? MaxRange { get; init; }

        // 기타 플래그 (Values.extra.pathDamage, aoeRange 등)
        public bool PathDamage { get; init; } = false;
        public int? AoeRange { get; init; }
        public bool IsAoe { get; init; } = false;
        public int? TargetLimit { get; init; }
    }
}
