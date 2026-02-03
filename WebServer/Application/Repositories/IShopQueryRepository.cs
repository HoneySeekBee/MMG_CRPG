using Domain.Enum;
using ShopEntity = Domain.Entities.Shop.Shop;

namespace Application.Repositories
{
    public interface IShopQueryRepository
    {
        // 목록 (유형별/활성별 필터, 이름·코드 검색, 페이징)
        Task<(IReadOnlyList<ShopEntity> Items, int TotalCount)> GetPagedAsync(
            ShopType? shopType,
            bool? isActive,
            string? search,
            int page,
            int pageSize,
            CancellationToken ct);

        // 상세 (Products 포함)
        Task<ShopEntity?> GetDetailAsync(int id, CancellationToken ct);

        // 현재 시점 기준 활성 상점 + 상품 (Game API용)
        Task<IReadOnlyList<ShopEntity>> GetActiveShopsAsync(DateTimeOffset now, CancellationToken ct);
    }
}
