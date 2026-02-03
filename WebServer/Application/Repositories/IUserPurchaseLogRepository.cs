using Domain.Entities.Shop;

namespace Application.Repositories
{
    public interface IUserPurchaseLogRepository
    {
        Task AddAsync(UserPurchaseLog entity, CancellationToken ct);

        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
