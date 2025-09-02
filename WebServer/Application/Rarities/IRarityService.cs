using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Rarities
{
    public interface IRarityService
    {
        Task<RarityDto?> GetAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<RarityDto>> ListAsync(bool? isActive, int? stars, int page, int pageSize, CancellationToken ct);
        Task<RarityDto> CreateAsync(CreateRarityRequest req, CancellationToken ct);
        Task UpdateAsync(int id, UpdateRarityRequest req, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
    }
}
