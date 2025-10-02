using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Roles
{
    public interface IRoleCache
    {
        IReadOnlyList<RoleDto> GetAll();
        RoleDto? GetById(int id);
        RoleDto? GetByKey(string key);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
