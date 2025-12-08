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
        None = 0,                 // 패시브 등 대상 없음
        Single = 1,               // 단일 대상 (기본)
        SingleNearest = 2,        // 가장 가까운 대상
        SingleFarthest = 3,       // 가장 멀리 있는 대상
        LowestHp = 4,             // HP 가장 낮은 대상
        HighestAtk = 5,           // 공격력 가장 높은 대상
        RandomOne = 6,            // 무작위 1명

        FrontN = 10,              // 앞에서 N명 (예: 베기)
        BackN = 11,               // 뒤에서 N명
        NearestN = 12,            // 가까운 N명
        FarthestN = 13,           // 먼 N명

        AllEnemies = 20,          // 적 전체
        AllAllies = 21,           // 아군 전체

        AreaCircle = 30,          // 원형 범위 (AoeRange)
        AreaSector = 31,          // 부채꼴 범위
        AreaRectangle = 32,       // 직선 범위
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
    public enum BuffKind : short
    {
        // 버프
        AtkUp,
        DefUp,
        SpdUp,
        CritRateUp,
        CritDamageUp,
        HpRegen,

        DamageReduce,        // 받는 피해 감소 %
        FinalDamageReduce,   // 최종 데미지 배율 감소
        FinalDamageIncrease, // 최종 데미지 배율 증가
        DefPenFlat,          // 방관 (고정 수치)
        DefPenPercent,       // 방관 (%)
        Shield,
        CooldownReduce,


        // 디버프
        AtkDown,
        DefDown,
        SpdDown,
        CritDown,

        Bleed,
        Burn,
        Poison,
        Stun,
        Silence,
        Freeze,
        Root,
        Knockdown
    }
}
