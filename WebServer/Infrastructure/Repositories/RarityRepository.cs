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
    public sealed class RarityRepository : IRarityRepository
    {
        private readonly GameDBContext _db;
        public RarityRepository(GameDBContext db) => _db = db;

        public Task<Rarity?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.Rarities.FirstOrDefaultAsync(x => x.RarityId == id, ct);

        public Task<Rarity?> GetByKeyAsync(string key, CancellationToken ct) =>
            _db.Rarities.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, ct);

        public async Task<IReadOnlyList<Rarity>> ListAsync(bool? isActive, int? stars, int skip, int take, CancellationToken ct)
        {
            var q = _db.Rarities.AsNoTracking().AsQueryable();
            if (isActive is not null) q = q.Where(x => x.IsActive == isActive);
            if (stars is not null) q = q.Where(x => x.Stars == stars);
            return await q
                .OrderBy(x => x.SortOrder).ThenBy(x => x.RarityId)
                .Skip(skip).Take(take)
                .ToListAsync(ct);
        }

        public Task AddAsync(Rarity entity, CancellationToken ct)
        { _db.Rarities.Add(entity); return Task.CompletedTask; }

        public Task RemoveAsync(Rarity entity, CancellationToken ct)
        { _db.Rarities.Remove(entity); return Task.CompletedTask; }

        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
