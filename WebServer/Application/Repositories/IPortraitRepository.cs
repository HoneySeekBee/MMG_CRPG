using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IPortraitRepository
    {
        Task<IReadOnlyList<Portrait>> GetAllAsync(CancellationToken ct);
        Task<Portrait?> GetByIdAsync(int id, CancellationToken ct);
        Task<Portrait?> GetByKeyAsync(string key, CancellationToken ct);
        Task AddAsync(Portrait entity, CancellationToken ct);
        Task UpdateAsync(Portrait entity, CancellationToken ct);
        Task DeleteAsync(Portrait entity, CancellationToken ct);
    }
}
