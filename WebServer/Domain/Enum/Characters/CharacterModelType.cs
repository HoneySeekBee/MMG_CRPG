using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum.Characters
{
    public enum BodySize
    {
        Small = 0,
        Normal = 1,
        Big = 2
    }

    public enum PartType
    {
        Head = 0,
        Hair = 1,
        Mouth = 2,
        Eye = 3,
        Acc = 4,
        WeaponL = 5,
        WeaponR = 6
    }

    // 애니메이션 상위 타입 (무기 스타일 다수 ↔ 1 애니메이션)
    public enum CharacterAnimationType
    {
        Bow = 0,
        OneHandSword = 1,
        Wand = 2,
        Fist = 3,
        TwoHandSword = 4,
        SwordShield = 5,
        Spear = 6
    }
}
