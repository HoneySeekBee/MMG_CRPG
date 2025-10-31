using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Characters
{
    public sealed class CharacterPromotion
    {
        // EF Core용
        private CharacterPromotion() { }

        // ==== Key ====
        public int CharacterId { get; private set; }
        public int Tier { get; private set; }          // >= 0

        // ==== Data ====
        public short MaxLevel { get; private set; }      // >= 1
        public StatModifier? Bonus { get; private set; } // JSONB 매핑 예정

        public ICollection<CharacterPromotionMaterial> Materials { get; } = new List<CharacterPromotionMaterial>();

        public int CostGold { get; private set; }        // >= 0

        // ==== Navigation (선택) ====
        public Character Character { get; private set; } = null!;

        // ==== Factory ====
        public static CharacterPromotion Create(
            int characterId,
            int tier,
            short maxLevel,
            int costGold,
            StatModifier? bonus = null,
            IEnumerable<PromotionMaterial>? materials = null)
        {
            if (tier < 0) throw new ArgumentOutOfRangeException(nameof(tier));
            if (maxLevel < 1) throw new ArgumentOutOfRangeException(nameof(maxLevel));
            if (costGold < 0) throw new ArgumentOutOfRangeException(nameof(costGold));

            var p = new CharacterPromotion
            {
                CharacterId = characterId,
                Tier = tier,
                MaxLevel = maxLevel,
                CostGold = costGold,
                Bonus = bonus
            };


            return p;
        }

        // ==== Behavior ====
        public void SetMaxLevel(short maxLevel)
        {
            if (maxLevel < 1) throw new ArgumentOutOfRangeException(nameof(maxLevel));
            MaxLevel = maxLevel;
        }

        public void SetCostGold(int gold)
        {
            if (gold < 0) throw new ArgumentOutOfRangeException(nameof(gold));
            CostGold = gold;
        }

        public void SetBonus(StatModifier? bonus) => Bonus = bonus;

    }
    public sealed record StatModifier(
        int? HP = null,
        int? ATK = null,
        int? DEF = null,
        int? SPD = null,
        decimal? CritRate = null,    // 5.00 = 5%
        decimal? CritDamage = null); // 150.00 = +150%

    public sealed record PromotionMaterial(int ItemId, int Quantity);
}
