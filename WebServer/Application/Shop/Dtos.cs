using Domain.Entities.Shop;
using Domain.Enum;

namespace Application.Shop
{
    public sealed record ShopDto(
        int Id,
        string Code,
        string Name,
        ShopType ShopType,
        DateTimeOffset? StartsAt,
        DateTimeOffset? EndsAt,
        bool IsActive,
        int SortOrder,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    // 상점 상세 (상품 포함)
    public sealed record ShopDetailDto(
        int Id,
        string Code,
        string Name,
        ShopType ShopType,
        DateTimeOffset? StartsAt,
        DateTimeOffset? EndsAt,
        bool IsActive,
        int SortOrder,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        IReadOnlyList<ShopProductDto> Products);

    public sealed record ShopProductDto(
        int Id,
        int ShopId,
        int ItemId,
        int CurrencyId,
        long Price,
        int QuantityPerPurchase,
        int? DailyLimit,
        int? WeeklyLimit,
        int? TotalLimit,
        int SortOrder,
        bool IsActive,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt);

    public sealed record PurchaseLogDto(
        long Id,
        int UserId,
        int ShopProductId,
        int Quantity,
        long PricePaid,
        string CurrencyCode,
        DateTimeOffset PurchasedAt);

    // 구매 결과
    public sealed record PurchaseResult
    {
        public bool Success { get; init; }
        public string? ErrorCode { get; init; }
        public long RemainingBalance { get; init; }
        public int ItemCount { get; init; }
        public int DailyPurchaseCount { get; init; }
        public int WeeklyPurchaseCount { get; init; }
        public int TotalPurchaseCount { get; init; }

        public static PurchaseResult Ok(
            long remainingBalance, int itemCount,
            int daily, int weekly, int total) => new()
        {
            Success = true,
            RemainingBalance = remainingBalance,
            ItemCount = itemCount,
            DailyPurchaseCount = daily,
            WeeklyPurchaseCount = weekly,
            TotalPurchaseCount = total
        };

        public static PurchaseResult Fail(string errorCode) => new()
        {
            Success = false,
            ErrorCode = errorCode
        };
    }

    // 에러 코드
    public static class ShopErrorCodes
    {
        public const string ShopNotFound = "SHOP_NOT_FOUND";
        public const string ShopNotActive = "SHOP_NOT_ACTIVE";
        public const string ShopNotInPeriod = "SHOP_NOT_IN_PERIOD";
        public const string ProductNotFound = "PRODUCT_NOT_FOUND";
        public const string ProductNotActive = "PRODUCT_NOT_ACTIVE";
        public const string DailyLimitExceeded = "DAILY_LIMIT_EXCEEDED";
        public const string WeeklyLimitExceeded = "WEEKLY_LIMIT_EXCEEDED";
        public const string TotalLimitExceeded = "TOTAL_LIMIT_EXCEEDED";
        public const string InsufficientCurrency = "INSUFFICIENT_CURRENCY";
        public const string PurchaseInProgress = "PURCHASE_IN_PROGRESS";
    }

    // Entity → DTO 매핑
    public static class ShopMappings
    {
        public static ShopDto ToDto(this Domain.Entities.Shop.Shop s) =>
            new(s.Id, s.Code, s.Name, s.ShopType, s.StartsAt, s.EndsAt,
                s.IsActive, s.SortOrder, s.CreatedAt, s.UpdatedAt);

        public static ShopDetailDto ToDetailDto(this Domain.Entities.Shop.Shop s) =>
            new(s.Id, s.Code, s.Name, s.ShopType, s.StartsAt, s.EndsAt,
                s.IsActive, s.SortOrder, s.CreatedAt, s.UpdatedAt,
                s.Products.Select(p => p.ToDto()).ToList());

        public static ShopProductDto ToDto(this ShopProduct p) =>
            new(p.Id, p.ShopId, p.ItemId, p.CurrencyId, p.Price, p.QuantityPerPurchase,
                p.DailyLimit, p.WeeklyLimit, p.TotalLimit, p.SortOrder, p.IsActive,
                p.CreatedAt, p.UpdatedAt);

        public static PurchaseLogDto ToDto(this UserPurchaseLog l) =>
            new(l.Id, l.UserId, l.ShopProductId, l.Quantity, l.PricePaid,
                l.CurrencyCode, l.PurchasedAt);
    }
}
