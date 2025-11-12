using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IIconRepository
    {
        Task<Icon?> GetByKeyAsync(string key, CancellationToken ct);
        Task<Icon?> GetByIdAsync(int id, CancellationToken ct);
        Task<List<Icon>> GetAllAsync(CancellationToken ct);
        Task AddAsync(Icon icon, CancellationToken ct);
        Task UpdateAsync(Icon icon, CancellationToken ct);
        Task DeleteAsync(Icon icon, CancellationToken ct);
    }
}
