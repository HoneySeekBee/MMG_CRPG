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

    public enum TriggerType : short
    {
        Auto = 0,
        Manual = 1,
        Passive = 2,
        OnHit = 3,
        OnDamaged = 4
    }

    public enum TargetingRule : short
    {
        None = 0,
        Self = 1,
        AllySingleLowestHp = 2,
        AllyAll = 3,
        EnemySingle = 4,
        EnemyAll = 5,
        EnemyBackline = 6
    }
}
