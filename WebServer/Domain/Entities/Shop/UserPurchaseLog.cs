using System;

namespace Domain.Entities.Shop
{
    public sealed class UserPurchaseLog
    {
        public long Id { get; private set; }
        public int UserId { get; private set; }
        public int ShopProductId { get; private set; }
        public int Quantity { get; private set; }
        public long PricePaid { get; private set; }
        public string CurrencyCode { get; private set; } = "";
        public DateTimeOffset PurchasedAt { get; private set; }

        private UserPurchaseLog() { }

        public static UserPurchaseLog Create(
            int userId,
            int shopProductId,
            int quantity,
            long pricePaid,
            string currencyCode,
            DateTimeOffset now)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (shopProductId <= 0) throw new ArgumentOutOfRangeException(nameof(shopProductId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (pricePaid < 0) throw new ArgumentOutOfRangeException(nameof(pricePaid));
            if (string.IsNullOrWhiteSpace(currencyCode)) throw new ArgumentException(nameof(currencyCode));

            return new UserPurchaseLog
            {
                UserId = userId,
                ShopProductId = shopProductId,
                Quantity = quantity,
                PricePaid = pricePaid,
                CurrencyCode = currencyCode.Trim(),
                PurchasedAt = now
            };
        }
    }
}
