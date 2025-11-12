using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IItemTypeRepository
    {
        Task<(IReadOnlyList<ItemType> Items, long Total)> SearchAsync(
            Application.ItemTypes.ListItemTypesRequest req, CancellationToken ct);

        Task<ItemType?> GetByIdAsync(short id, bool includeSlot = false, CancellationToken ct = default);
        Task AddAsync(ItemType e, CancellationToken ct);
        Task RemoveAsync(ItemType e, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
