using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum SkillType : short
    {
        Unknown = 0,
        Attack = 1,
        Heal = 2,
        Support = 3,
        Buff = 4,
        Debuff = 5
    }
    public enum SkillTargetingType : short
    {
        None = 0,
        Targeting = 1,
        NoneTargeting = 2,
    }

    public enum AoeShapeType : short
    {
        None = 0,
        Circle = 1, 
    }
    public enum TargetSideType : short
    {
        None = 0,
        Team = 1,
        Enemy = 2, 
    }
}
