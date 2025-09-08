using Application.Repositories;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class GachaBannerRepository : IGachaBannerRepository
    {
        private readonly GameDBContext _db;

        public GachaBannerRepository(GameDBContext db) => _db = db;

        public Task<GachaBanner?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.GachaBanners.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<GachaBanner?> GetByKeyAsync(string key, CancellationToken ct = default)
            => _db.GachaBanners.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, ct);

        public async Task<IReadOnlyList<GachaBanner>> ListLiveAsync(
            DateTimeOffset? now = null, int take = 10, CancellationToken ct = default)
        {
            var t = now ?? DateTimeOffset.UtcNow;

            return await _db.GachaBanners.AsNoTracking()
                .Where(b => b.IsActive
                            && b.Status == GachaBannerStatus.Live
                            && b.StartsAt <= t
                            && (b.EndsAt == null || b.EndsAt > t))
                .OrderByDescending(b => b.Priority)
                .ThenByDescending(b => b.Id)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<(IReadOnlyList<GachaBanner> Items, int Total)> SearchAsync(
            string? keyword = null, int skip = 0, int take = 20, CancellationToken ct = default)
        {
            keyword = (keyword ?? string.Empty).Trim();
            var q = _db.GachaBanners.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                var k = keyword.ToLower();
                q = q.Where(b =>
                    b.Title.ToLower().Contains(k) ||
                    b.Key.ToLower().Contains(k));
            }
            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(b => b.Id)
                               .Skip(skip).Take(take)
                               .ToListAsync(ct);
            return (items, total);
        }

        public async Task AddAsync(GachaBanner banner, CancellationToken ct = default)
        {
            await _db.GachaBanners.AddAsync(banner, ct);
        }

        public Task UpdateAsync(GachaBanner banner, CancellationToken ct = default)
        {
            _db.GachaBanners.Update(banner);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.GachaBanners.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity != null) _db.GachaBanners.Remove(entity);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
