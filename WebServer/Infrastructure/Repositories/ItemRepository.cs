using Application.Items;
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
    public sealed class ItemRepository : IItemRepository
    {
        private readonly GameDBContext _db;
        public ItemRepository(GameDBContext db) => _db = db;

        // ---------- 조회 ----------

        public async Task<Item?> GetByIdAsync(long id, bool includeChildren, CancellationToken ct)
        {
            IQueryable<Item> q = _db.Items;
            if (includeChildren)
                q = q
                    .Include(i => i.Stats)
                    .Include(i => i.Effects)
                    .Include(i => i.Prices);

            return await q.FirstOrDefaultAsync(i => i.Id == id, ct);
        }

        public async Task<Item?> GetByCodeAsync(string code, bool includeChildren, CancellationToken ct)
        {
            IQueryable<Item> q = _db.Items;
            if (includeChildren)
                q = q
                    .Include(i => i.Stats)
                    .Include(i => i.Effects)
                    .Include(i => i.Prices);

            return await q.FirstOrDefaultAsync(i => i.Code == code, ct);
        }

        public async Task<(IReadOnlyList<Item> Items, long TotalCount)> SearchAsync(
            ListItemsRequest req, CancellationToken ct)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var size = req.PageSize <= 0 ? 50 : req.PageSize;
            var q = _db.Items.AsQueryable();

            if (req.TypeId is { } typeId)
                q = q.Where(i => i.TypeId == typeId);

            if (req.RarityId is { } rarityId)
                q = q.Where(i => i.RarityId == rarityId);

            if (req.IsActive is { } active)
                q = q.Where(i => i.IsActive == active);

            if (!string.IsNullOrWhiteSpace(req.Search))
            {
                var s = req.Search.Trim().ToLower();
                q = q.Where(i => i.Code.ToLower().Contains(s)
                              || i.Name.ToLower().Contains(s));
            }

            if (req.Tags is { Length: > 0 })
            {
                var tags = req.Tags!;
                q = q.Where(i => i.Tags.Any(t => tags.Contains(t)));
            }


            // 정렬: 간단 기본값(희귀도, 타입, 코드)
            q = req.Sort?.ToLowerInvariant() switch
            {
                "code" => q.OrderBy(i => i.Code),
                "name" => q.OrderBy(i => i.Name),
                "rarity" => q.OrderBy(i => i.RarityId).ThenBy(i => i.Code),
                "type" => q.OrderBy(i => i.TypeId).ThenBy(i => i.Code),
                "created" => q.OrderBy(i => i.CreatedAt),
                "updated" => q.OrderBy(i => i.UpdatedAt),
                _ => q.OrderBy(i => i.RarityId).ThenBy(i => i.TypeId).ThenBy(i => i.Code)
            };

            var total = await q.LongCountAsync(ct);

            var items = await q
                .Skip((page - 1) * size)
                .Take(size)
                .AsNoTracking()
                .ToListAsync(ct);

            return (items, total);
        }

        // ---------- 명령 ----------

        public Task AddAsync(Item item, CancellationToken ct)
        {
            _db.Items.Add(item);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Item item, CancellationToken ct)
        {
            _db.Items.Remove(item);
            return Task.CompletedTask;
        }

        public async Task<bool> IsCodeUniqueAsync(string code, long? excludeId, CancellationToken ct)
        {
            var q = _db.Items.AsQueryable().Where(i => i.Code == code);
            if (excludeId is { } id) q = q.Where(i => i.Id != id);
            return !await q.AnyAsync(ct);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
