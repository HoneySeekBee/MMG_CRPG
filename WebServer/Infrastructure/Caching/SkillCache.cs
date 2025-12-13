using Application.Combat;
using Application.SkillLevels;
using Application.Skills;
using Domain.Entities.Skill;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public class SkillCache : ISkillCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        public SkillCache(IDbContextFactory<GameDBContext> factory) => _factory = factory;

        private List<SkillWithLevelsDto> _cache = new();
        private Dictionary<int, SkillWithLevelsDto> _byId = new();

        // Skill과 SkillLevel 테이블
        public IReadOnlyList<SkillWithLevelsDto> GetAll() => _cache;
        public SkillWithLevelsDto? GetById(int id) => _cache.FirstOrDefault(x => x.SkillId == id);
        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            // 1) DB에서 스킬 + 레벨 DTO만 가져오기 (Effect는 일단 null/기본값)
            var list = await db.Skills
                .AsNoTracking()
                .OrderBy(s => s.SkillId)
                .Include(s => s.Levels)
                .Select(s => new SkillWithLevelsDto
                {
                    SkillId = s.SkillId,
                    Name = s.Name,
                    Type = s.Type,
                    ElementId = s.ElementId,
                    IconId = s.IconId,
                    IsActive = s.IsActive,
                    TargetingType = s.TargetingType,
                    TargetSide = s.TargetSide,
                    AoeShape = s.AoeShape,
                    Tag = s.Tag ?? Array.Empty<string>(),
                    BaseInfo = s.BaseInfo,

                    Levels = s.Levels
                        .OrderBy(l => l.Level)
                        .Select(l => new SkillLevelDto
                        {
                            SkillId = l.SkillId,
                            Level = l.Level,
                            Values = l.Values,
                            Description = l.Description,
                            Materials = l.Materials,
                            CostGold = l.CostGold,
                            ParentType = s.Type,
                            IsPassive = !s.IsActive,
                        })
                        .ToList(),
                     
                    Effect = new SkillEffect()
                })
                .ToListAsync(ct);

            // 2) 메모리에서 대표 레벨(max level) Values로 Effect 파싱
            foreach (var dto in list)
            {
                var maxValues = dto.Levels
                    .OrderByDescending(x => x.Level)
                    .Select(x => x.Values)
                    .FirstOrDefault();
                dto.Effect = SkillEffectParser.SafeParseEffect(dto.SkillId, maxValues);
            }

            _cache = list;
            Console.WriteLine($"스킬 캐싱하기 {list.Count}개 ");
            _byId = list.ToDictionary(x => x.SkillId);
        }
    }
}
