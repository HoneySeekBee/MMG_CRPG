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
    public sealed class SkillLevelRepository : ISkillLevelRepository
    {
        private readonly GameDBContext _db;

        public SkillLevelRepository(GameDBContext db) => _db = db;

        public async Task<SkillLevel?> GetByIdAsync(int skillId, int level, CancellationToken ct)
        {
            return await _db.SkillLevels
                .FirstOrDefaultAsync(x => x.SkillId == skillId && x.Level == level, ct);
        }

        public async Task<IReadOnlyList<SkillLevel>> ListAsync(int skillId, CancellationToken ct)
        {
            return await _db.SkillLevels
                .Where(x => x.SkillId == skillId)
                .OrderBy(x => x.Level)
                .ToListAsync(ct);
        }

        public async Task AddAsync(SkillLevel entity, CancellationToken ct)
        {
            await _db.SkillLevels.AddAsync(entity, ct);
        }

        public Task RemoveAsync(SkillLevel entity, CancellationToken ct)
        {
            _db.SkillLevels.Remove(entity);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
