using Application.Factions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public sealed class FactionCache : IFactionCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private readonly object _gate = new();

        private List<FactionDto> _all = new();
        private Dictionary<int, FactionDto> _byId = new();
        private Dictionary<string, FactionDto> _byKey = new(StringComparer.OrdinalIgnoreCase);

        public FactionCache(IDbContextFactory<GameDBContext> factory) => _factory = factory;

        public IReadOnlyList<FactionDto> GetAll() => _all;
        public FactionDto? GetById(int id) => _byId.TryGetValue(id, out var v) ? v : null;
        public FactionDto? GetByKey(string key) => _byKey.TryGetValue(key, out var v) ? v : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);
            var entities = await db.Factions.AsNoTracking()
    .OrderBy(x => x.SortOrder).ThenBy(x => x.FactionId)
    .ToListAsync(ct);

            var list = entities.Select(FactionDto.From).ToList();
            var byId = list.ToDictionary(x => x.FactionId);
            var byKey = list.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

            lock (_gate)
            {
                _all = list;
                _byId = byId;
                _byKey = byKey;
            }

            Console.WriteLine($"[FactionCache] loaded: {_all.Count}");
        }
    }
}
