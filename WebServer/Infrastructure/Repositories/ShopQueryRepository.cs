using Application.Repositories;
using Domain.Entities.Shop;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class ShopQueryRepository : IShopQueryRepository
    {
        private readonly GameDBContext _db;

        public ShopQueryRepository(GameDBContext db) => _db = db;

        public async Task<(IReadOnlyList<Shop> Items, int TotalCount)> GetPagedAsync(
            ShopType? shopType,
            bool? isActive,
            string? search,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            var q = _db.Shops.AsNoTracking().AsQueryable();

            if (shopType.HasValue)
                q = q.Where(x => x.ShopType == shopType.Value);

            if (isActive.HasValue)
                q = q.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                q = q.Where(x =>
                    EF.Functions.Like(x.Name, $"%{term}%") ||
                    EF.Functions.Like(x.Code, $"%{term}%"));
            }

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public Task<Shop?> GetDetailAsync(int id, CancellationToken ct)
            => _db.Shops
                  .AsNoTracking()
                  .Include(x => x.Products.OrderBy(p => p.SortOrder))
                  .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<IReadOnlyList<Shop>> GetActiveShopsAsync(DateTimeOffset now, CancellationToken ct)
        {
            var shops = await _db.Shops
                .AsNoTracking()
                .Include(x => x.Products.Where(p => p.IsActive).OrderBy(p => p.SortOrder))
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .ToListAsync(ct);

            // 기간 한정 상점은 운영 기간 내인 것만 필터
            return shops
                .Where(x => x.IsOpenAt(now))
                .ToList();
        }
    }
}
