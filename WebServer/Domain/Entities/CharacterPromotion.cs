using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class CharacterPromotion
    {
        // EF Core용
        private CharacterPromotion() { }

        // ==== Key ====
        public int CharacterId { get; private set; }
        public short Tier { get; private set; }          // >= 0

        // ==== Data ====
        public short MaxLevel { get; private set; }      // >= 1
        public StatModifier? Bonus { get; private set; } // JSONB 매핑 예정

        // JSONB 매핑 예정(리스트)
        private readonly List<PromotionMaterial> _materials = new();
        public IReadOnlyList<PromotionMaterial> Materials => _materials;

        public int CostGold { get; private set; }        // >= 0

        // ==== Navigation (선택) ====
        public Character Character { get; private set; } = null!;

        // ==== Factory ====
        public static CharacterPromotion Create(
            int characterId,
            short tier,
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

            if (materials != null) p.ReplaceMaterials(materials);

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

        public void ReplaceMaterials(IEnumerable<PromotionMaterial>? materials)
        {
            _materials.Clear();
            if (materials == null) return;

            // 같은 ItemId는 합산하여 정규화
            foreach (var group in materials
                         .Where(m => m.Quantity > 0 && m.ItemId > 0)
                         .GroupBy(m => m.ItemId))
            {
                var qty = group.Sum(g => g.Quantity);
                if (qty > 0) _materials.Add(new PromotionMaterial(group.Key, qty));
            }
        }

        public void AddMaterial(int itemId, int quantity)
        {
            if (itemId <= 0) throw new ArgumentOutOfRangeException(nameof(itemId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            var idx = _materials.FindIndex(m => m.ItemId == itemId);
            if (idx >= 0)
                _materials[idx] = _materials[idx] with { Quantity = _materials[idx].Quantity + quantity };
            else
                _materials.Add(new PromotionMaterial(itemId, quantity));
        }

        public void RemoveMaterial(int itemId)
        {
            var idx = _materials.FindIndex(m => m.ItemId == itemId);
            if (idx >= 0) _materials.RemoveAt(idx);
        }

        public void ClearMaterials() => _materials.Clear();
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
