using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Elements
{
    public interface IElementService
    {
        Task<int> CreateAsync(CreateElementRequest req, CancellationToken ct);
        Task UpdateAsync(int id, UpdateElementRequest req, CancellationToken ct);
        Task SetActiveAsync(int id, bool isActive, CancellationToken ct);
        Task<ElementDto> GetByIdAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<ElementDto>> ListAsync(bool? isActive, string? search, int page, int pageSize, CancellationToken ct);
    }
}
