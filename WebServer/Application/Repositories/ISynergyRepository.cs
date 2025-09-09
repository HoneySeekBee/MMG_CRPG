using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DmSynergy = Domain.Entities.Synergy;

namespace Application.Repositories
{
    public interface ISynergyRepository
    {
        Task<DmSynergy?> GetAsync(int id, CancellationToken ct = default);
        Task<DmSynergy?> GetByKeyAsync(string key, CancellationToken ct = default);
        Task<IReadOnlyList<DmSynergy>> GetActiveAsync(DateTime now, CancellationToken ct = default);

        Task AddAsync(DmSynergy synergy, CancellationToken ct = default);
        Task UpdateAsync(DmSynergy synergy, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
