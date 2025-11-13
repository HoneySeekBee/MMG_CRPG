using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Monsters
{
    public class MonsterStatProgression
    {
        // 복합키 중 FK 부분
        public int MonsterId { get; private set; } 
        // 레벨/단계
        public int Level { get; private set; }

        // 능력치
        public int HP { get; private set; }
        public int ATK { get; private set; }
        public int DEF { get; private set; }
        public int SPD { get; private set; }

        // numeric(5,2)
        public decimal CritRate { get; private set; }
        public decimal CritDamage { get; private set; }
        public float Range { get; private set; }

        // 네비게이션 (역방향)
        public Monster Monster { get; private set; } = null!;

        private MonsterStatProgression() { } // EF

        public MonsterStatProgression(
            int level,
            int hp,
            int atk,
            int def,
            int spd,
            decimal critRate,
            decimal critDamage,
            float range)
        {
            Level = level;
            HP = hp;
            ATK = atk;
            DEF = def;
            SPD = spd;
            CritRate = critRate;
            CritDamage = critDamage;
            Range = range;
        }

        public void Update(
            int hp,
            int atk,
            int def,
            int spd,
            decimal critRate,
            decimal critDamage, 
            float range)
        {
            HP = hp;
            ATK = atk;
            DEF = def;
            SPD = spd;
            CritRate = critRate;
            CritDamage = critDamage;
            Range = range;
        }
    }
}
