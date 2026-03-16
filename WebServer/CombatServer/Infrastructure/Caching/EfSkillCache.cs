using Application.Combat;
using Application.SkillLevels;
using Application.Skills;
using Domain.Enum;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Text.Json.Nodes;

namespace Infrastructure.Caching
{
    public sealed class EfSkillCache : ISkillCache
    {
        private readonly IDbContextFactory<CombatDbContext> _factory;
        private volatile IReadOnlyList<SkillWithLevelsDto> _all = Array.Empty<SkillWithLevelsDto>();
        private readonly ConcurrentDictionary<int, SkillWithLevelsDto> _byId = new();

        public EfSkillCache(IDbContextFactory<CombatDbContext> factory) => _factory = factory;

        public IReadOnlyList<SkillWithLevelsDto> GetAll() => _all;

        public SkillWithLevelsDto? GetById(int id) =>
            _byId.TryGetValue(id, out var s) ? s : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            var rows = await db.SkillRows.AsNoTracking().ToListAsync(ct);

            var list = rows.Select(r =>
            {
                JsonNode? baseInfo = null;
                if (!string.IsNullOrEmpty(r.BaseInfo))
                {
                    try { baseInfo = JsonNode.Parse(r.BaseInfo); } catch { }
                }

                var dto = new SkillWithLevelsDto
                {
                    SkillId = r.SkillId,
                    Type = (SkillType)r.Type,
                    TargetingType = (SkillTargetingType)r.TargetingType,
                    AoeShape = (AoeShapeType)r.AoeShape,
                    TargetSide = (TargetSideType)r.TargetSide,
                    BaseInfo = baseInfo,
                    Levels = Array.Empty<SkillLevelDto>(),
                    Effect = SkillEffectParser.SafeParseEffect(r.SkillId, baseInfo),
                };
                return dto;
            }).ToList();

            _byId.Clear();
            foreach (var s in list)
                _byId[s.SkillId] = s;

            _all = list;
        }
    }
}
