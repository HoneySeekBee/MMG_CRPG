using System;

namespace Domain.Entities.Shop
{
    public sealed class ShopProduct
    {
        public int Id { get; private set; }
        public int ShopId { get; private set; }
        public int ItemId { get; private set; }
        public int CurrencyId { get; private set; }
        public long Price { get; private set; }
        public int QuantityPerPurchase { get; private set; } = 1;
        public int? DailyLimit { get; private set; }
        public int? WeeklyLimit { get; private set; }
        public int? TotalLimit { get; private set; }
        public int SortOrder { get; private set; }
        public bool IsActive { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private ShopProduct() { }

        public ShopProduct(
            int shopId,
            int itemId,
            int currencyId,
            long price,
            int quantityPerPurchase = 1,
            int? dailyLimit = null,
            int? weeklyLimit = null,
            int? totalLimit = null,
            int sortOrder = 0,
            bool isActive = true)
        {
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
            if (quantityPerPurchase <= 0) throw new ArgumentOutOfRangeException(nameof(quantityPerPurchase));

            ShopId = shopId;
            ItemId = itemId;
            CurrencyId = currencyId;
            Price = price;
            QuantityPerPurchase = quantityPerPurchase;
            DailyLimit = dailyLimit;
            WeeklyLimit = weeklyLimit;
            TotalLimit = totalLimit;
            SortOrder = sortOrder;
            IsActive = isActive;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Update(
            int itemId,
            int currencyId,
            long price,
            int quantityPerPurchase,
            int? dailyLimit,
            int? weeklyLimit,
            int? totalLimit,
            int sortOrder,
            bool isActive)
        {
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
            if (quantityPerPurchase <= 0) throw new ArgumentOutOfRangeException(nameof(quantityPerPurchase));

            ItemId = itemId;
            CurrencyId = currencyId;
            Price = price;
            QuantityPerPurchase = quantityPerPurchase;
            DailyLimit = dailyLimit;
            WeeklyLimit = weeklyLimit;
            TotalLimit = totalLimit;
            SortOrder = sortOrder;
            IsActive = isActive;
            Touch();
        }

        public void Activate() { IsActive = true; Touch(); }
        public void Deactivate() { IsActive = false; Touch(); }

        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }
}
