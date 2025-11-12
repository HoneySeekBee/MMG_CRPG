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
    public class ElementRepository : IElementRepository
    {
        private readonly GameDBContext _db;
        public ElementRepository(GameDBContext db) => _db = db;

        public Task<Element?> GetByIdAsync(int id, CancellationToken ct)
            => _db.Elements.FirstOrDefaultAsync(x => x.ElementId == id, ct);

        public Task<Element?> GetByKeyAsync(string key, CancellationToken ct)
            => _db.Elements.FirstOrDefaultAsync(x => x.Key == key, ct);

        public Task<bool> KeyExistsAsync(string key, CancellationToken ct)
            => _db.Elements.AnyAsync(x => x.Key == key, ct);

        public async Task AddAsync(Element e, CancellationToken ct)
        {
            await _db.Elements.AddAsync(e, ct);
            await _db.SaveChangesAsync(ct); // 단순 패턴: 여기서 저장
        }

        public async Task RemoveAsync(Element e, CancellationToken ct)
        {
            _db.Elements.Remove(e);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<Element>> ListAsync(bool? isActive, string? search, int skip, int take, CancellationToken ct)
        {
            var q = _db.Elements.AsQueryable();
            if (isActive is not null) q = q.Where(x => x.IsActive == isActive);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(x => x.Key.Contains(s) || x.Label.Contains(s));
            }
            return await q.OrderBy(x => x.SortOrder).ThenBy(x => x.ElementId)
                          .Skip(skip).Take(take)
                          .ToListAsync(ct);
        }
        public Task SaveChangesAsync(CancellationToken ct)
    => _db.SaveChangesAsync(ct);
    }
}
