using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public static class DamageFormula
    {
        private static readonly Random _rng = new();
        public static int ComputeBase(int atk, int def)
        {
            float fAtk = atk;
            float fDef = Math.Max(def, 0);

            float guaranteedBlock = fDef / 5f;
            float remainingDef = fDef * 4f / 5f;
            float rate = MathF.Min(fDef / 100f, 0.9f);
            float extraBlock = remainingDef * rate;

            float rawDamage = fAtk - (guaranteedBlock + extraBlock);

            return Math.Max(1, (int)MathF.Round(rawDamage));
        }
        public static int ComputeWithCrit(int atk, int def, double critRate, double critDamage, out bool isCrit)
        {
            int baseDamage = ComputeBase(atk, def);

            isCrit = _rng.NextDouble() < critRate;

            if (isCrit)
            {
                return (int)MathF.Round(baseDamage * (1f + (float)critDamage));
            }

            return baseDamage;
        }
    }
}
