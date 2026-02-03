using Domain.Enum;

namespace Application.Shop
{
    public sealed record CreateShopRequest(
        string Code,
        string Name,
        ShopType ShopType,
        DateTimeOffset? StartsAt,
        DateTimeOffset? EndsAt,
        bool IsActive,
        int SortOrder = 0);

    public sealed record UpdateShopRequest(
        string Name,
        ShopType ShopType,
        DateTimeOffset? StartsAt,
        DateTimeOffset? EndsAt,
        bool IsActive,
        int SortOrder);

    public sealed record CreateShopProductRequest(
        int ItemId,
        int CurrencyId,
        long Price,
        int QuantityPerPurchase = 1,
        int? DailyLimit = null,
        int? WeeklyLimit = null,
        int? TotalLimit = null,
        int SortOrder = 0,
        bool IsActive = true);

    public sealed record UpdateShopProductRequest(
        int ItemId,
        int CurrencyId,
        long Price,
        int QuantityPerPurchase,
        int? DailyLimit,
        int? WeeklyLimit,
        int? TotalLimit,
        int SortOrder,
        bool IsActive);

    public sealed record ShopListFilter(
        ShopType? ShopType = null,
        bool? IsActive = null,
        string? Search = null,
        int Page = 1,
        int PageSize = 20);

    public sealed record PurchaseLogFilter(
        int? UserId = null,
        int? ShopProductId = null,
        DateTimeOffset? From = null,
        DateTimeOffset? To = null,
        int Page = 1,
        int PageSize = 20);

    public sealed record PurchaseRequest(
        int ShopProductId,
        int Quantity = 1);
}
