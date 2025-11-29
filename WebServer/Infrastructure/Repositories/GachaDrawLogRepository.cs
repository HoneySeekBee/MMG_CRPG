using Application.Repositories;
using Domain.Entities.Gacha;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class GachaDrawLogRepository : IGachaDrawLogRepository
    {
        private readonly GameDBContext _db;

        public GachaDrawLogRepository(GameDBContext db)
        {
            _db = db;
        }

        public Task AddAsync(GachaDrawLog log, CancellationToken ct = default)
            => _db.GachaDrawLogs.AddAsync(log, ct).AsTask();

        public async Task<IReadOnlyList<GachaDrawLog>> GetRecentAsync(
            int userId,
            int take = 20,
            CancellationToken ct = default)
        {
            return await _db.GachaDrawLogs
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Timestamp)
                .Take(take)
                .ToListAsync(ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
