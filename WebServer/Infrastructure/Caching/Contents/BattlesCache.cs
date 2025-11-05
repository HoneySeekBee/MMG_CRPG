using Application.Contents.Battles;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching.Contents
{
    public class BattlesCache : IBattlesCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private readonly object _gate = new();

        private List<BattleDto> _all = new();
        private Dictionary<int, BattleDto> _byId = new();

        public BattlesCache(IDbContextFactory<GameDBContext> factory)
        {
            _factory = factory;
        }

        public IReadOnlyList<BattleDto> GetAll() => _all;

        public BattleDto? GetById(int id) =>
            _byId.TryGetValue(id, out var v) ? v : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            // DB 컬럼: id, name, active, scene_key, check_multi, created_at, updated_at
            var list = await db.Battles
                .AsNoTracking()
                .OrderBy(b => b.Id)
                .Select(b => new BattleDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Active = b.Active,
                    SceneKey = b.SceneKey,
                    CheckMulti = b.CheckMulti,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                })
                .ToListAsync(ct);

            var byId = list.ToDictionary(x => x.Id);

            // 원자적 스왑
            lock (_gate)
            {
                _all = list;
                _byId = byId;
            }

            Console.WriteLine($"[BattlesCache] loaded: {_all.Count}");
        }
    }
}
