using Application.Repositories;
using Domain.Entities.Shop;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class UserPurchaseLogQueryRepository : IUserPurchaseLogQueryRepository
    {
        private readonly GameDBContext _db;

        public UserPurchaseLogQueryRepository(GameDBContext db) => _db = db;

        public async Task<PurchaseCounts> GetPurchaseCountsAsync(
            int userId,
            int productId,
            DateTimeOffset todayUtcStart,
            DateTimeOffset weekStartUtc,
            CancellationToken ct)
        {
            var result = await _db.UserPurchaseLogs
                .Where(x => x.UserId == userId && x.ShopProductId == productId)
                .GroupBy(x => 1)
                .Select(g => new PurchaseCounts(
                    g.Count(x => x.PurchasedAt >= todayUtcStart),
                    g.Count(x => x.PurchasedAt >= weekStartUtc),
                    g.Count()))
                .FirstOrDefaultAsync(ct);

            return result ?? PurchaseCounts.Zero;
        }

        public async Task<(IReadOnlyList<UserPurchaseLog> Items, int TotalCount)> GetLogsPagedAsync(
            int? userId,
            int? shopProductId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            var q = _db.UserPurchaseLogs.AsNoTracking().AsQueryable();

            if (userId.HasValue)
                q = q.Where(x => x.UserId == userId.Value);

            if (shopProductId.HasValue)
                q = q.Where(x => x.ShopProductId == shopProductId.Value);

            if (from.HasValue)
                q = q.Where(x => x.PurchasedAt >= from.Value);

            if (to.HasValue)
                q = q.Where(x => x.PurchasedAt <= to.Value);

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(x => x.PurchasedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }
    }
}
