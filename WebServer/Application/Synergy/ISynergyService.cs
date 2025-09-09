using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Synergy
{
    public interface ISynergyService
    {
        Task<SynergyDto> CreateAsync(CreateSynergyRequest req, CancellationToken ct = default);
        Task<SynergyDto?> GetAsync(int id, CancellationToken ct = default);
        Task<SynergyDto?> GetByKeyAsync(string key, CancellationToken ct = default);
        Task<IReadOnlyList<SynergyDto>> GetActivesAsync(DateTime now, CancellationToken ct = default);
        Task<SynergyDto> UpdateAsync(UpdateSynergyRequest req, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<EvaluateResult>> EvaluateAsync(EvaluateSynergiesRequest req, CancellationToken ct = default);
    }
}
