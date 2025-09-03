using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class CharacterStatProgression
    {
        // EF Core용
        private CharacterStatProgression() { }

        // ==== Key ====
        public int CharacterId { get; private set; }
        public short Level { get; private set; }        // >= 1

        // ==== Base Stats (정수 권장) ====
        public int HP { get; private set; }            // >= 0
        public int ATK { get; private set; }            // >= 0
        public int DEF { get; private set; }            // >= 0
        public int SPD { get; private set; }            // >= 0

        // ==== Crit (퍼센트값; 5 = 5%) ====
        public decimal CritRate { get; private set; } = 5m;    // 0..100
        public decimal CritDamage { get; private set; } = 150m;  // 0..1000 (예: 150 = +150%)

        // ==== Navigation ====
        public Character Character { get; private set; } = null!;

        // ==== Factory ====
        public static CharacterStatProgression Create(
            int characterId,
            short level,
            int hp,
            int atk,
            int def,
            int spd,
            decimal? critRate = null,
            decimal? critDamage = null)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            if (hp < 0 || atk < 0 || def < 0 || spd < 0)
                throw new ArgumentOutOfRangeException("stats must be >= 0");

            var cr = critRate ?? 5m;
            var cd = critDamage ?? 150m;

            if (cr < 0 || cr > 100) throw new ArgumentOutOfRangeException(nameof(critRate));
            if (cd < 0 || cd > 1000) throw new ArgumentOutOfRangeException(nameof(critDamage));

            return new CharacterStatProgression
            {
                CharacterId = characterId,
                Level = level,
                HP = hp,
                ATK = atk,
                DEF = def,
                SPD = spd,
                CritRate = cr,
                CritDamage = cd
            };
        }

        // ==== Mutators ====
        public void SetBaseStats(int hp, int atk, int def, int spd)
        {
            if (hp < 0 || atk < 0 || def < 0 || spd < 0)
                throw new ArgumentOutOfRangeException("stats must be >= 0");

            HP = hp; ATK = atk; DEF = def; SPD = spd;
        }

        public void SetCrit(decimal critRate, decimal critDamage)
        {
            if (critRate < 0 || critRate > 100)
                throw new ArgumentOutOfRangeException(nameof(critRate));
            if (critDamage < 0 || critDamage > 1000)
                throw new ArgumentOutOfRangeException(nameof(critDamage));

            CritRate = critRate;
            CritDamage = critDamage;
        }
    }
}
