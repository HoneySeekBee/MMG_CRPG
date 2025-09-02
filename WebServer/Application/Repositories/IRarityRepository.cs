using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IRarityRepository
    {
        Task<Rarity?> GetByIdAsync(int id, CancellationToken ct);
        Task<Rarity?> GetByKeyAsync(string key, CancellationToken ct);
        Task<IReadOnlyList<Rarity>> ListAsync(bool? isActive, int? stars, int skip, int take, CancellationToken ct);

        Task AddAsync(Rarity entity, CancellationToken ct);
        Task RemoveAsync(Rarity entity, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
