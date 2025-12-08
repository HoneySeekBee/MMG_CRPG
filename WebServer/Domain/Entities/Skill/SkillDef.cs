using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Skill
{
    public sealed class SkillDef
    {
        public int SkillId { get; init; }
        public string Name { get; init; }
        public SkillType Type { get; init; }
        public TargetSideType TargetSide { get; init; }
        public SkillTargetingType TargetingType { get; init; }
        public AoeShapeType AoeShape { get; init; }

        public SkillEffect Effect { get; init; }
    }
}
