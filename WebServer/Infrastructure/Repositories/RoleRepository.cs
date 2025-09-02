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
    public sealed class RoleRepository : IRoleRepository
    {
        private readonly GameDBContext _db;
        public RoleRepository(GameDBContext db) => _db = db;

        public Task<Role?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.Roles.FirstOrDefaultAsync(x => x.RoleId == id, ct);

        public Task<Role?> GetByKeyAsync(string key, CancellationToken ct) =>
            _db.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, ct);

        public async Task<IReadOnlyList<Role>> ListAsync(bool? isActive, int skip, int take, CancellationToken ct)
        {
            var q = _db.Roles.AsNoTracking().AsQueryable();
            if (isActive is not null) q = q.Where(x => x.IsActive == isActive);
            return await q
                .OrderBy(x => x.SortOrder).ThenBy(x => x.RoleId)
                .Skip(skip).Take(take)
                .ToListAsync(ct);
        }

        public Task AddAsync(Role entity, CancellationToken ct)
        { _db.Roles.Add(entity); return Task.CompletedTask; }

        public Task RemoveAsync(Role entity, CancellationToken ct)
        { _db.Roles.Remove(entity); return Task.CompletedTask; }

        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
