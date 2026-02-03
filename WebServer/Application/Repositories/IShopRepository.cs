using ShopEntity = Domain.Entities.Shop.Shop;

namespace Application.Repositories
{
    public interface IShopRepository
    {
        Task<ShopEntity?> GetByIdAsync(int id, CancellationToken ct);

        // Products 포함 조회 (상점 수정 시 사용)
        Task<ShopEntity?> GetByIdWithProductsAsync(int id, CancellationToken ct);

        // 코드 중복 체크 (수정 시 excludeId로 자기 자신 제외)
        Task<bool> ExistsCodeAsync(string code, int? excludeId, CancellationToken ct);

        Task AddAsync(ShopEntity entity, CancellationToken ct);

        void Remove(ShopEntity entity);

        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
