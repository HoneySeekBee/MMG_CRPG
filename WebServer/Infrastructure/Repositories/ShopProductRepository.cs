using Application.Repositories;
using Domain.Entities.Shop;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class ShopProductRepository : IShopProductRepository
    {
        private readonly GameDBContext _db;

        public ShopProductRepository(GameDBContext db) => _db = db;

        public Task<ShopProduct?> GetByIdAsync(int id, CancellationToken ct)
            => _db.ShopProducts.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<IReadOnlyList<ShopProduct>> GetByShopIdAsync(int shopId, CancellationToken ct)
            => await _db.ShopProducts
                  .Where(x => x.ShopId == shopId)
                  .OrderBy(x => x.SortOrder)
                  .ToListAsync(ct);

        public async Task AddAsync(ShopProduct entity, CancellationToken ct)
        {
            await _db.ShopProducts.AddAsync(entity, ct);
        }

        public void Remove(ShopProduct entity)
        {
            _db.ShopProducts.Remove(entity);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
