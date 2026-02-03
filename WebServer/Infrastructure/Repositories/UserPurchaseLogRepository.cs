using Application.Repositories;
using Domain.Entities.Shop;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public sealed class UserPurchaseLogRepository : IUserPurchaseLogRepository
    {
        private readonly GameDBContext _db;

        public UserPurchaseLogRepository(GameDBContext db) => _db = db;

        public async Task AddAsync(UserPurchaseLog entity, CancellationToken ct)
        {
            await _db.UserPurchaseLogs.AddAsync(entity, ct);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
