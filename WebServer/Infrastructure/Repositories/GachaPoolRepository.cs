using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class GachaPoolRepository : IGachaPoolRepository
    {
        private readonly GameDBContext _db;
        public GachaPoolRepository(GameDBContext db) => _db = db;

        // ─────────── 조회 ───────────
        public Task<GachaPool?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.GachaPools.AsNoTracking()
                             .FirstOrDefaultAsync(p => p.PoolId == id, ct);
        public Task<GachaPool?> GetWithEntriesAsync(int id, CancellationToken ct = default)
    => _db.GachaPools
          .Include(p => p.Entries)
          .AsNoTracking() // 또는 AsNoTrackingWithIdentityResolution()
          .FirstOrDefaultAsync(p => p.PoolId == id, ct);

        public async Task<(IReadOnlyList<GachaPool> Items, int Total)> SearchAsync(
            string? keyword = null, int skip = 0, int take = 20, CancellationToken ct = default)
        {
            keyword = (keyword ?? string.Empty).Trim();
            var q = _db.GachaPools.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                var k = keyword.ToLower();
                q = q.Where(p => p.Name.ToLower().Contains(k) ||
                                 (p.TablesVersion != null && p.TablesVersion.ToLower().Contains(k)));
            }

            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(p => p.ScheduleStart)
                               .ThenByDescending(p => p.PoolId)
                               .Skip(skip).Take(take)
                               .ToListAsync(ct);

            return (items, total);
        }

        public async Task<IReadOnlyList<GachaPool>> ListAsync(int take = 100, CancellationToken ct = default)
        {
            return await _db.GachaPools.AsNoTracking()
                .OrderByDescending(p => p.ScheduleStart)
                .ThenByDescending(p => p.PoolId)
                .Take(take)
                .ToListAsync(ct);
        }

        // ─────────── CUD ───────────
        public async Task AddAsync(GachaPool pool, CancellationToken ct = default)
            => await _db.GachaPools.AddAsync(pool, ct);

        public Task UpdateAsync(GachaPool pool, CancellationToken ct = default)
        {
            _db.GachaPools.Update(pool);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.GachaPools.FirstOrDefaultAsync(x => x.PoolId == id, ct);
            if (entity != null) _db.GachaPools.Remove(entity);
        }

        // ─────────── 엔트리 벌크 교체 ───────────
        public async Task ReplaceEntriesAsync(int poolId, IEnumerable<GachaPoolEntry> entries, CancellationToken ct = default)
        {
            // 1) 기존 것 제거
            var olds = await _db.GachaPoolEntries
                .Where(x => x.PoolId == poolId)
                .ToListAsync(ct);
            _db.GachaPoolEntries.RemoveRange(olds);

            // 2) 새로 추가 (PoolId 주입)
            var list = entries?.ToList() ?? new();
            foreach (var e in list)
            {
                // public setter가 없다면 Entry API로 주입
                _db.Entry(e).Property(nameof(GachaPoolEntry.PoolId)).CurrentValue = poolId;
            }
            await _db.GachaPoolEntries.AddRangeAsync(list, ct);

            // 3) 커밋은 한 번만 → EF 내부 트랜잭션 사용
            await _db.SaveChangesAsync(ct);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
