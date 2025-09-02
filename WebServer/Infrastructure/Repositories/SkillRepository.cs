using Application.Repositories;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class SkillRepository : ISkillRepository
    {
        private readonly GameDBContext _db;

        public SkillRepository(GameDBContext db) => _db = db;

        public async Task<Skill?> GetByIdAsync(int id, bool includeLevels, CancellationToken ct)
        {
            IQueryable<Skill> q = _db.Skills;
            if (includeLevels)
                q = q.Include(s => s.Levels);
            return await q.FirstOrDefaultAsync(s => s.SkillId == id, ct);
        }

        public async Task<Skill?> GetByNameAsync(string name, CancellationToken ct)
        {
            return await _db.Skills.FirstOrDefaultAsync(s => s.Name == name, ct);
        }

        public async Task<IReadOnlyList<Skill>> ListAsync(
            SkillType? type,
            int? elementId,
            string? nameContains,
            int skip,
            int take,
            CancellationToken ct)
        {
            var q = _db.Skills.AsQueryable();

            if (type.HasValue) q = q.Where(s => s.Type == type.Value);
            if (elementId.HasValue) q = q.Where(s => s.ElementId == elementId.Value);
            if (!string.IsNullOrWhiteSpace(nameContains))
            {
                var n = nameContains.Trim(); 
                q = q.Where(s => s.Name.Contains(n));
            }

            q = q.OrderBy(s => s.SkillId);

            return await q.Skip(skip).Take(take).ToListAsync(ct);
        }

        public async Task AddAsync(Skill entity, CancellationToken ct)
        {
            await _db.Skills.AddAsync(entity, ct);
        }

        public Task RemoveAsync(Skill entity, CancellationToken ct)
        {
            _db.Skills.Remove(entity);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
