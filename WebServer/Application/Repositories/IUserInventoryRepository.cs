using Application.UserInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInven = Domain.Entities.User.UserInventory;

namespace Application.Repositories
{
    public interface IUserInventoryRepository
    {
        Task<UserInven?> GetByKeyAsync(int userId, int itemId, CancellationToken ct);
        Task AddAsync(UserInven entity, CancellationToken ct);
        void Remove(UserInven entity);
        Task<int> SaveChangesAsync(CancellationToken ct); 
        Task AddItemAsync(int userId, long itemId, int amount, CancellationToken ct);
    }

    public interface IUserInventoryQueryRepository
    {
        // UserInventoryListQuery → (rows, total)
        Task<(IReadOnlyList<UserInven> Rows, int Total)> GetPagedAsync(UserInventoryListQuery query, CancellationToken ct);

        // ItemOwnershipQuery → (rows, total)
        Task<(IReadOnlyList<UserInven> Rows, int Total)> GetOwnersPagedAsync(ItemOwnershipQuery query, CancellationToken ct);
    }
}
