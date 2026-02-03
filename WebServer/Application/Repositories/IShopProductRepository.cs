using Domain.Entities.Shop;

namespace Application.Repositories
{
    public interface IShopProductRepository
    {
        Task<ShopProduct?> GetByIdAsync(int id, CancellationToken ct);

        Task<IReadOnlyList<ShopProduct>> GetByShopIdAsync(int shopId, CancellationToken ct);

        Task AddAsync(ShopProduct entity, CancellationToken ct);

        void Remove(ShopProduct entity);

        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
