using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class CharacterPromotionMaterial
    {
        private CharacterPromotionMaterial() { }
        public int PromotionCharacterId { get; private set; }
        public int PromotionTier { get; private set; }
        public long ItemId { get; private set; }
        public int Count { get; private set; }
        public CharacterPromotion Promotion { get; private set; } = null!;
        public Item Item { get; private set; } = null!;

        public static CharacterPromotionMaterial Create(int characterId, int tier, long itemId, int count)
            => new() { PromotionCharacterId = characterId, PromotionTier = tier, ItemId = itemId, Count = count };
    }
}
