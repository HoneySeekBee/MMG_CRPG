using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Factions
{
    public interface IFactionService
    {
        Task<FactionDto?> GetAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<FactionDto>> ListAsync(bool? isActive, int page, int pageSize, CancellationToken ct);
        Task<FactionDto> CreateAsync(CreateFactionRequest req, CancellationToken ct);
        Task UpdateAsync(int id, UpdateFactionRequest req, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
    }
}
