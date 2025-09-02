using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Roles
{
    public interface IRoleService
    {
        Task<RoleDto?> GetAsync(int id, CancellationToken ct);
        Task<IReadOnlyList<RoleDto>> ListAsync(bool? isActive, int page, int pageSize, CancellationToken ct);
        Task<RoleDto> CreateAsync(CreateRoleRequest req, CancellationToken ct);
        Task UpdateAsync(int id, UpdateRoleRequest req, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
    }
}
