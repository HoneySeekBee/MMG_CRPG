using Application.ItemTypes;
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
    public sealed class EFItemTypeRepository : IItemTypeRepository
    {
        private readonly GameDBContext _db;
        public EFItemTypeRepository(GameDBContext db) => _db = db;

        public async Task<(IReadOnlyList<ItemType> Items, long Total)> SearchAsync(ListItemTypesRequest req, CancellationToken ct)
        {
            var q = _db.ItemTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(req.Search))
            {
                var s = req.Search.Trim();
                var sLower = s.ToLower();

                q = q.Where(x =>
                    EF.Functions.Like(x.Code.ToLower(), $"%{sLower}%") ||
                    EF.Functions.Like(x.Name.ToLower(), $"%{sLower}%"));
            }
            if (req.HasSlot is { } hs)
                q = hs ? q.Where(x => x.SlotId != null) : q.Where(x => x.SlotId == null);

            // sort
            q = (req.Sort?.ToLowerInvariant(), req.Desc) switch
            {
                ("name", true) => q.OrderByDescending(x => x.Name),
                ("name", _) => q.OrderBy(x => x.Name),
                ("slot", true) => q.OrderByDescending(x => x.SlotId).ThenBy(x => x.Name),
                ("slot", _) => q.OrderBy(x => x.SlotId).ThenBy(x => x.Name),
                ("created", true) => q.OrderByDescending(x => x.CreatedAt),
                ("created", _) => q.OrderBy(x => x.CreatedAt),
                ("updated", true) => q.OrderByDescending(x => x.UpdatedAt),
                ("updated", _) => q.OrderBy(x => x.UpdatedAt),
                (_, true) => q.OrderByDescending(x => x.Code),
                _ => q.OrderBy(x => x.Code)
            };

            var total = await q.LongCountAsync(ct);
            var items = await q
                .Skip((req.Page - 1) * req.PageSize)
                .Take(req.PageSize)
                .Include(x => x.Slot)
                .AsNoTracking()
                .ToListAsync(ct);

            return (items, total);
        }

        public Task<ItemType?> GetByIdAsync(short id, bool includeSlot = false, CancellationToken ct = default) =>
            includeSlot
                ? _db.ItemTypes.Include(x => x.Slot).FirstOrDefaultAsync(x => x.Id == id, ct)
                : _db.ItemTypes.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task AddAsync(ItemType e, CancellationToken ct)
        {
            _db.ItemTypes.Add(e);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(ItemType e, CancellationToken ct)
        {
            _db.ItemTypes.Remove(e);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
