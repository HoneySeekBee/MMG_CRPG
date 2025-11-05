using Domain.Entities.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories.Contents
{
    public interface IBattlesRepository
    {
        Task<Battle?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Battle>> GetListAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Battle battle, CancellationToken cancellationToken = default);
        Task UpdateAsync(Battle battle, CancellationToken cancellationToken = default);
        Task DeleteAsync(Battle battle, CancellationToken cancellationToken = default);
    }
}
