using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(int id, CancellationToken ct);
        Task<Role?> GetByKeyAsync(string key, CancellationToken ct);
        Task<IReadOnlyList<Role>> ListAsync(bool? isActive, int skip, int take, CancellationToken ct);

        Task AddAsync(Role entity, CancellationToken ct);
        Task RemoveAsync(Role entity, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
