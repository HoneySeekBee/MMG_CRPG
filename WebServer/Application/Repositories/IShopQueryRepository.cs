using Domain.Entities.Shop;
using Domain.Enum;

namespace Application.Repositories
{
    public interface IShopQueryRepository
    {
        // 목록 (유형별/활성별 필터, 이름·코드 검색, 페이징)
        Task<(IReadOnlyList<Shop> Items, int TotalCount)> GetPagedAsync(
            ShopType? shopType,
            bool? isActive,
            string? search,
            int page,
            int pageSize,
            CancellationToken ct);

        // 상세 (Products 포함)
        Task<Shop?> GetDetailAsync(int id, CancellationToken ct);

        // 현재 시점 기준 활성 상점 + 상품 (Game API용)
        Task<IReadOnlyList<Shop>> GetActiveShopsAsync(DateTimeOffset now, CancellationToken ct);
    }
}
