using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ElementAffinities
{
    public interface IElementAffinityService
    {
        Task<ElementAffinityDto?> GetAsync(int attacker, int defender, CancellationToken ct);
        Task<IReadOnlyList<ElementAffinityDto>> ListAsync(
            int? attacker, int? defender, int page, int pageSize, CancellationToken ct);
        Task CreateAsync(CreateElementAffinityRequest req, CancellationToken ct);
        Task UpdateAsync(int attacker, int defender, UpdateElementAffinityRequest req, CancellationToken ct);
        Task DeleteAsync(int attacker, int defender, CancellationToken ct);
    }
}
