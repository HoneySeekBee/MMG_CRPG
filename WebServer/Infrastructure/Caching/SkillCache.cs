using Application.Combat;
using Application.SkillLevels;
using Application.Skills;
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

                    // 레벨을 같은 Select에서 즉시 투영 (DB 한 방)
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

                            // 부모 정보가 필요하면 여기서 채워두기
                            ParentType = s.Type,
                            IsPassive = !s.IsActive, // 규칙에 맞게 조정
                        })
                        .ToList(),

                    Effect = SkillEffectParser.Parse(s)
                })
                // 레벨 수가 많다면 SplitQuery가 도움이 될 수 있음 (상황에 따라)
                //.AsSplitQuery()
                .ToListAsync(ct);

            // 원자적 스왑
            _cache = list;
            Console.WriteLine($"스킬 캐싱하기 {list.Count}개 ");
            _byId = list.ToDictionary(x => x.SkillId);
        }
         
    }
}
