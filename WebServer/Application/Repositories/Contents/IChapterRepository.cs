using Domain.Entities.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories.Contents
{
    public interface IChapterRepository
    {
        Task<Chapter?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Chapter>> GetListAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Chapter chapter, CancellationToken cancellationToken = default);
        Task UpdateAsync(Chapter chapter, CancellationToken cancellationToken = default);
        Task DeleteAsync(Chapter chapter, CancellationToken cancellationToken = default);
    }
}
