using Application.Portraits;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public sealed class PortraitCache : IPortraitsCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private List<PortraitMetaDto> _cache = new();

        public PortraitCache(IDbContextFactory<GameDBContext> factory) => _factory = factory;

        public IReadOnlyList<PortraitMetaDto> GetAll() => _cache;
        public PortraitMetaDto? GetById(int id) => _cache.FirstOrDefault(x => x.PortraitId == id);
        public PortraitMetaDto? GetByKey(string key) => _cache.FirstOrDefault(x => x.Key == key);

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);
            _cache = await db.Portraits
                .Select(x => new PortraitMetaDto(x.PortraitId, x.Key, x.Version))
                .ToListAsync(ct);

            Console.WriteLine($"[PortraitCache] loaded: {_cache.Count}");
        }
    }
}
