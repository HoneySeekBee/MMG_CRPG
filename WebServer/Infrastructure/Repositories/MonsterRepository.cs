using Application.Monsters;
using Domain.Entities.Monsters;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class MonsterRepository : IMonsterRepository
    {
        private readonly GameDBContext _db;

        public MonsterRepository(GameDBContext db)
        {
            _db = db;
        }

        public async Task<List<Monster>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Monsters
                .Include(m => m.Stats)
                .AsNoTracking()
                .OrderBy(m => m.Id)
                .ToListAsync(ct);
        }

        public async Task<Monster?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Monsters
                .Include(m => m.Stats)
                .FirstOrDefaultAsync(m => m.Id == id, ct);
        }

        public async Task AddAsync(Monster monster, CancellationToken ct = default)
        {
            await _db.Monsters.AddAsync(monster, ct);
        }

        public async Task DeleteAsync(Monster monster, CancellationToken ct = default)
        {
            _db.Monsters.Remove(monster);
            await Task.CompletedTask; // 일관성 위해 async 패턴 유지
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
