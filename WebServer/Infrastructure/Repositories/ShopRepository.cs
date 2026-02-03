using Application.Repositories;
using Domain.Entities.Shop;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class ShopRepository : IShopRepository
    {
        private readonly GameDBContext _db;

        public ShopRepository(GameDBContext db) => _db = db;

        public Task<Shop?> GetByIdAsync(int id, CancellationToken ct)
            => _db.Shops.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<Shop?> GetByIdWithProductsAsync(int id, CancellationToken ct)
            => _db.Shops
                  .Include(x => x.Products.OrderBy(p => p.SortOrder))
                  .FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<bool> ExistsCodeAsync(string code, int? excludeId, CancellationToken ct)
        {
            var q = _db.Shops.AsNoTracking().Where(x => x.Code == code);
            if (excludeId.HasValue)
                q = q.Where(x => x.Id != excludeId.Value);
            return q.AnyAsync(ct);
        }

        public async Task AddAsync(Shop entity, CancellationToken ct)
        {
            await _db.Shops.AddAsync(entity, ct);
        }

        public void Remove(Shop entity)
        {
            _db.Shops.Remove(entity);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
