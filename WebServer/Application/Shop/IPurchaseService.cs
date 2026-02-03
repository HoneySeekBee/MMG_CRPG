namespace Application.Shop
{
    public interface IPurchaseService
    {
        // 활성 상점 + 유저별 잔여 구매 횟수 (Game API용)
        Task<IReadOnlyList<UserShopDto>> GetShopListForUserAsync(int userId, CancellationToken ct);

        // 구매
        Task<PurchaseResult> PurchaseAsync(int userId, int shopProductId, int quantity, CancellationToken ct);
    }

    // 유저별 상점 + 상품 구매 현황
    public sealed record UserShopDto(
        ShopDto Shop,
        IReadOnlyList<UserShopProductDto> Products);

    public sealed record UserShopProductDto(
        ShopProductDto Product,
        int DailyPurchased,
        int WeeklyPurchased,
        int TotalPurchased);
}
