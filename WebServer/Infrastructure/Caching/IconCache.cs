using Application.Icons;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public sealed class IconCache : IIconCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private List<IconMetaDto> _cache = new();

        public IconCache(IDbContextFactory<GameDBContext> factory) => _factory = factory;

        public IReadOnlyList<IconMetaDto> GetAll() => _cache;
        public IconMetaDto? GetById(int id) => _cache.FirstOrDefault(x => x.IconId == id);
        public IconMetaDto? GetByKey(string key) => _cache.FirstOrDefault(x => x.Key == key);

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);
            // 실제 테이블/엔티티 이름에 맞게 수정
            _cache = await db.Icons
                .Select(x => new IconMetaDto(x.IconId, x.Key, x.Version))
                .ToListAsync(ct);

            Console.WriteLine($"[IconCache] loaded: {_cache.Count}");
        }
    }
}
