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
    public sealed class EfPortraitRepository : IPortraitRepository
    {
        private readonly GameDBContext _db;
        public EfPortraitRepository(GameDBContext db) => _db = db;

        // 인터페이스와 동일한 시그니처
        public async Task<IReadOnlyList<Portrait>> GetAllAsync(CancellationToken ct)
            => await _db.Portraits.AsNoTracking().ToListAsync(ct);

        public Task<Portrait?> GetByIdAsync(int id, CancellationToken ct)
            => _db.Portraits.FirstOrDefaultAsync(p => p.PortraitId == id, ct);

        public Task<Portrait?> GetByKeyAsync(string key, CancellationToken ct)
            => _db.Portraits.FirstOrDefaultAsync(p => p.Key == key, ct);

        public async Task AddAsync(Portrait portrait, CancellationToken ct)
        {
            _db.Portraits.Add(portrait);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Portrait portrait, CancellationToken ct)
        {
            // 필요 시 아래처럼 확실히 수정 상태로 표시:
            // _db.Portraits.Update(portrait);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Portrait portrait, CancellationToken ct)
        {
            _db.Portraits.Remove(portrait);
            await _db.SaveChangesAsync(ct);
        }
    }
}
