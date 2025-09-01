using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IElementAffinityRepository
    {
        Task<ElementAffinity?> GetAsync(int attacker, int defender, CancellationToken ct);
        Task AddAsync(ElementAffinity entity, CancellationToken ct);
        Task RemoveAsync(ElementAffinity entity, CancellationToken ct);

        Task<IReadOnlyList<ElementAffinity>> ListAsync(
            int? attacker, int? defender, int skip, int take, CancellationToken ct);

        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
