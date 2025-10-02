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
        private readonly GameDBContext _db;

        private Dictionary<int, RarityDto> _byId = new();
        private Dictionary<string, RarityDto> _byKey = new();
        private List<RarityDto> _all = new();

        public RarityCache(GameDBContext db)
        {
            _db = db;
        }

        public IReadOnlyList<RarityDto> GetAll() => _all;
        public RarityDto? GetById(int id) => _byId.TryGetValue(id, out var dto) ? dto : null;
        public RarityDto? GetByKey(string key) => _byKey.TryGetValue(key, out var dto) ? dto : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            var entities = await _db.Rarities.AsNoTracking().ToListAsync(ct);

            var list = entities.Select(RarityDto.From).ToList();
            _all = list;
            _byId = list.ToDictionary(x => x.RarityId);
            _byKey = list.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        }
    }
}
