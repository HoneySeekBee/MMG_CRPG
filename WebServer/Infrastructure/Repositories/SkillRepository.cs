using Application.Repositories;
using Domain.Entities.Skill;
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
            IQueryable<Skill> q = _db.Skills.AsQueryable();
            if (includeLevels) q = q.Include(s => s.Levels);

            return await q.FirstOrDefaultAsync(s => s.SkillId == id, ct);
        }

        public async Task<Skill?> GetByNameAsync(string name, CancellationToken ct)
        {
            return await _db.Skills.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Name == name, ct);
        }
        public async Task<IReadOnlyList<Skill>> ListAsync(
        SkillType? type,
        int? elementId,
        string? nameContains,
        int skip,
        int take,
        CancellationToken ct)
        {
            var q = _db.Skills.AsNoTracking().AsQueryable();

            if (type.HasValue) q = q.Where(s => s.Type == type.Value);
            if (elementId.HasValue) q = q.Where(s => s.ElementId == elementId.Value);
            if (!string.IsNullOrWhiteSpace(nameContains))
            {
                var n = nameContains.Trim();
                // Postgres 대소문자 무시: ILIKE
                q = q.Where(s => s.Name.Contains(n));
            }

            // 기본 정렬
            q = q.OrderBy(s => s.SkillId);

            return await q.Skip(skip).Take(take).ToListAsync(ct);
        }
        public async Task<PagedResult<Skill>> ListAsync(SkillListFilter filter, CancellationToken ct)
        {
            var q = _db.Skills.AsNoTracking().AsQueryable();

            if (filter.Type.HasValue) q = q.Where(s => s.Type == filter.Type);
            if (filter.ElementId.HasValue) q = q.Where(s => s.ElementId == filter.ElementId);
            if (filter.IsActive.HasValue) q = q.Where(s => s.IsActive == filter.IsActive);
            if (filter.TargetingType.HasValue) q = q.Where(s => s.TargetingType == filter.TargetingType);
            if (filter.TargetSide.HasValue) q = q.Where(s => s.TargetSide == filter.TargetSide);
            if (filter.AoeShape.HasValue) q = q.Where(s => s.AoeShape == filter.AoeShape);

            if (!string.IsNullOrWhiteSpace(filter.NameContains))
            {
                var n = filter.NameContains.Trim();
                q = q.Where(s => s.Name.Contains(n));
            }

            // TagsAll: 모두 포함
            if (filter.TagsAll is { Length: > 0 })
            {
                // 모든 태그가 s.Tag에 포함되는지
                foreach (var t in filter.TagsAll.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    var tag = t.Trim().ToLowerInvariant();
                    q = q.Where(s => s.Tag.Contains(tag)); // text[] contains element
                }
            }

            // TagsAny: 하나라도 포함
            if (filter.TagsAny is { Length: > 0 })
            {
                var any = filter.TagsAny
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Trim().ToLowerInvariant())
                    .ToArray();

                if (any.Length > 0)
                    q = q.Where(s => s.Tag.Any(t => any.Contains(t)));
            }

            // 정렬
            q = (filter.SortBy, filter.Desc) switch
            {
                (SkillSortBy.Name, false) => q.OrderBy(s => s.Name),
                (SkillSortBy.Name, true) => q.OrderByDescending(s => s.Name),
                (SkillSortBy.Type, false) => q.OrderBy(s => s.Type),
                (SkillSortBy.Type, true) => q.OrderByDescending(s => s.Type),
                (SkillSortBy.ElementId, false) => q.OrderBy(s => s.ElementId),
                (SkillSortBy.ElementId, true) => q.OrderByDescending(s => s.ElementId),
                (SkillSortBy.TargetingType, false) => q.OrderBy(s => s.TargetingType),
                (SkillSortBy.TargetingType, true) => q.OrderByDescending(s => s.TargetingType),
                (SkillSortBy.TargetSide, false) => q.OrderBy(s => s.TargetSide),
                (SkillSortBy.TargetSide, true) => q.OrderByDescending(s => s.TargetSide),
                (SkillSortBy.AoeShape, false) => q.OrderBy(s => s.AoeShape),
                (SkillSortBy.AoeShape, true) => q.OrderByDescending(s => s.AoeShape),
                (SkillSortBy.IsActive, false) => q.OrderBy(s => s.IsActive),
                (SkillSortBy.IsActive, true) => q.OrderByDescending(s => s.IsActive),
                _ => q.OrderBy(s => s.SkillId)
            };

            // TotalCount (먼저 계산)
            var total = await q.CountAsync(ct);

            // 페이징 (서버 상한선 권장: Math.Min(filter.Take, 200))
            var items = await q
                .Skip(filter.Skip)
                .Take(filter.Take)
                .ToListAsync(ct);

            return new PagedResult<Skill> { Items = items, TotalCount = total };
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
