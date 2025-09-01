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
    public class ElementAffinityRepository: IElementAffinityRepository
    {
        private readonly GameDBContext _db;
        public ElementAffinityRepository(GameDBContext db) => _db = db;

        public Task<ElementAffinity?> GetAsync(int attacker, int defender, CancellationToken ct)
            => _db.Set<ElementAffinity>()
                  .FirstOrDefaultAsync(x => x.AttackerElementId == attacker
                                          && x.DefenderElementId == defender, ct);

        public async Task AddAsync(ElementAffinity entity, CancellationToken ct)
            => await _db.Set<ElementAffinity>().AddAsync(entity, ct);

        public Task RemoveAsync(ElementAffinity entity, CancellationToken ct)
        {
            _db.Set<ElementAffinity>().Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<ElementAffinity>> ListAsync(
            int? attacker, int? defender, int skip, int take, CancellationToken ct)
        {
            var q = _db.Set<ElementAffinity>().AsQueryable();

            if (attacker is not null) q = q.Where(x => x.AttackerElementId == attacker);
            if (defender is not null) q = q.Where(x => x.DefenderElementId == defender);

            return await q.OrderBy(x => x.AttackerElementId)
                          .ThenBy(x => x.DefenderElementId)
                          .Skip(skip).Take(take)
                          .ToListAsync(ct);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
