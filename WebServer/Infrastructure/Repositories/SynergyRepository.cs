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
    public sealed class SynergyRepository : ISynergyRepository
    {
        private readonly GameDBContext _db;
        public SynergyRepository(GameDBContext db) => _db = db;

        public async Task<Synergy?> GetAsync(int id, CancellationToken ct)
            => await _db.Synergies
                .Include(s => s.Bonuses)
                .Include(s => s.Rules)
                .FirstOrDefaultAsync(s => s.SynergyId == id, ct);

        public async Task<Synergy?> GetByKeyAsync(string key, CancellationToken ct)
            => await _db.Synergies
                .Include(s => s.Bonuses)
                .Include(s => s.Rules)
                .FirstOrDefaultAsync(s => s.Key == key, ct);

        public async Task<IReadOnlyList<Synergy>> GetActiveAsync(DateTime now, CancellationToken ct)
            => await _db.Synergies
                .Where(s => s.IsActive &&
                            (!s.StartAt.HasValue || s.StartAt <= now) &&
                            (!s.EndAt.HasValue || now <= s.EndAt))
                .Include(s => s.Bonuses)
                .Include(s => s.Rules)
                .AsNoTracking()
                .ToListAsync(ct);

        public async Task AddAsync(Synergy synergy, CancellationToken ct)
        {
            _db.Synergies.Add(synergy);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Synergy synergy, CancellationToken ct)
        {
            _db.Synergies.Update(synergy); // 부분 갱신이면 Entry.Property(..).IsModified 플래그 사용
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _db.Synergies.FindAsync(new object?[] { id }, ct);
            if (entity is null) return;
            _db.Synergies.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}
