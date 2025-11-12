using Application.Rarities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public sealed class RarityCache : IRarityCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private readonly object _gate = new();

        private List<RarityDto> _all = new();
        private Dictionary<int, RarityDto> _byId = new();
        private Dictionary<string, RarityDto> _byKey = new(StringComparer.OrdinalIgnoreCase);

        public RarityCache(IDbContextFactory<GameDBContext> factory) => _factory = factory;

        public IReadOnlyList<RarityDto> GetAll() => _all;
        public RarityDto? GetById(int id) => _byId.TryGetValue(id, out var v) ? v : null;
        public RarityDto? GetByKey(string key) => _byKey.TryGetValue(key, out var v) ? v : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);
            var entities = await db.Rarities.AsNoTracking()
    .OrderBy(x => x.SortOrder).ThenBy(x => x.RarityId)
    .ToListAsync(ct);
            var list = entities.Select(RarityDto.From).ToList();
            var byId = list.ToDictionary(x => x.RarityId);
            var byKey = list.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

            lock (_gate)
            {
                _all = list;
                _byId = byId;
                _byKey = byKey;
            }

            Console.WriteLine($"[RarityCache] loaded: {_all.Count}");
        }
    }
}
