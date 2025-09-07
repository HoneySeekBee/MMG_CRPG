using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IStatTypeRepository
    {
        Task<List<StatType>> ListAsync(CancellationToken ct);
        Task<StatType?> GetByIdAsync(short id, CancellationToken ct);
        Task<StatType?> GetByCodeAsync(string code, CancellationToken ct);

        Task AddAsync(StatType entity, CancellationToken ct);
        Task RemoveAsync(StatType entity, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
