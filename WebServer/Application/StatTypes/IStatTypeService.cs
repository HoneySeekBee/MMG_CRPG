using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.StatTypes
{
    public interface IStatTypeService
    {
        Task<IReadOnlyList<StatTypeDto>> ListAsync(CancellationToken ct);
        Task<StatTypeDto?> GetAsync(short id, CancellationToken ct);
        Task<StatTypeDto> CreateAsync(CreateStatTypeRequest req, CancellationToken ct);
        Task<StatTypeDto> UpdateAsync(short id, UpdateStatTypeRequest req, CancellationToken ct);
        Task DeleteAsync(short id, CancellationToken ct);
    }
}
