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
    public sealed class EFStatTypeRepository : IStatTypeRepository
    {
        private readonly GameDBContext _db;
        public EFStatTypeRepository(GameDBContext db) => _db = db;

        public Task<List<StatType>> ListAsync(CancellationToken ct)
            => _db.StatTypes.AsNoTracking().ToListAsync(ct);

        public Task<StatType?> GetByIdAsync(short id, CancellationToken ct)
            => _db.StatTypes.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<StatType?> GetByCodeAsync(string code, CancellationToken ct)
            => _db.StatTypes.FirstOrDefaultAsync(x => x.Code == code, ct);

        public Task AddAsync(StatType entity, CancellationToken ct)
        { _db.StatTypes.Add(entity); return Task.CompletedTask; }

        public Task RemoveAsync(StatType entity, CancellationToken ct)
        { _db.StatTypes.Remove(entity); return Task.CompletedTask; }

        public Task<int> SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
