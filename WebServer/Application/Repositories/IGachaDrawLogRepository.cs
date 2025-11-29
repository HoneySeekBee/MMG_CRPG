using Domain.Entities.Gacha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IGachaDrawLogRepository
    {
        Task AddAsync(GachaDrawLog log, CancellationToken ct = default);

        Task<IReadOnlyList<GachaDrawLog>> GetRecentAsync(int userId, int take = 20, CancellationToken ct = default);

        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
