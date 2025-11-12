using Application.Monsters;
using Domain.Entities.Monsters;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public sealed class MonsterCache : IMonsterCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private readonly ILogger<MonsterCache> _logger;

        private ImmutableDictionary<int, MonsterDto> _byId
            = ImmutableDictionary<int, MonsterDto>.Empty;
        private IReadOnlyList<MonsterDto> _all = Array.Empty<MonsterDto>();

        public MonsterCache(IDbContextFactory<GameDBContext> factory,
                            ILogger<MonsterCache> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public IReadOnlyList<MonsterDto> GetAll() => _all;

        public MonsterDto? GetById(int id)
            => _byId.TryGetValue(id, out var dto) ? dto : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Reloading MonsterCache...");

            await using var db = await _factory.CreateDbContextAsync(ct);

            var monsters = await db.Set<Monster>()
                .AsNoTracking()
                .Include(m => m.Stats)
                .ToListAsync(ct);

            var dtos = monsters
                .Select(m => new MonsterDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    ModelKey = m.ModelKey,
                    ElementId = m.ElementId,
                    PortraitId = m.PortraitId,
                    Stats = m.Stats
                        .OrderBy(s => s.Level)
                        .Select(s => new MonsterStatDto
                        {
                            MonsterId = s.MonsterId,
                            Level = s.Level,
                            HP = s.HP,
                            ATK = s.ATK,
                            DEF = s.DEF,
                            SPD = s.SPD,
                            CritRate = s.CritRate,
                            CritDamage = s.CritDamage
                        })
                        .ToList()
                })
                .OrderBy(d => d.Id)
                .ToList();

            _byId = dtos.ToImmutableDictionary(d => d.Id, d => d);
            _all = dtos;
            Console.WriteLine($"MonsterCache reloaded: {dtos.Count} monsters.");
            _logger.LogInformation("MonsterCache reloaded: {Count} monsters.", dtos.Count);
        }
    }
} 
