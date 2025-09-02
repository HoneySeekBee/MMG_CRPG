using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IFactionRepository
    {
        Task<Faction?> GetByIdAsync(int id, CancellationToken ct);
        Task<Faction?> GetByKeyAsync(string key, CancellationToken ct);
        Task<IReadOnlyList<Faction>> ListAsync(bool? isActive, int skip, int take, CancellationToken ct);

        Task AddAsync(Faction entity, CancellationToken ct);
        Task RemoveAsync(Faction entity, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
