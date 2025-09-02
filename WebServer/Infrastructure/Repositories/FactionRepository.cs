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
    public sealed class FactionRepository : IFactionRepository
    {
        private readonly GameDBContext _db;
        public FactionRepository(GameDBContext db) => _db = db;

        public Task<Faction?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.Factions.AsNoTracking().FirstOrDefaultAsync(x => x.FactionId == id, ct);

        public Task<Faction?> GetByKeyAsync(string key, CancellationToken ct) =>
            _db.Factions.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, ct);

        public async Task<IReadOnlyList<Faction>> ListAsync(bool? isActive, int skip, int take, CancellationToken ct)
        {
            var q = _db.Factions.AsNoTracking().AsQueryable();
            if (isActive is not null) q = q.Where(x => x.IsActive == isActive);
            return await q
                .OrderBy(x => x.SortOrder).ThenBy(x => x.FactionId)
                .Skip(skip).Take(take)
                .ToListAsync(ct);
        }

        public Task AddAsync(Faction entity, CancellationToken ct)
        { _db.Factions.Add(entity); return Task.CompletedTask; }

        public Task RemoveAsync(Faction entity, CancellationToken ct)
        { _db.Factions.Remove(entity); return Task.CompletedTask; }

        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
