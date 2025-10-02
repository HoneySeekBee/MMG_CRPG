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
        private readonly GameDBContext _db;
        private Dictionary<int, FactionDto> _byId = new();
        private Dictionary<string, FactionDto> _byKey = new();
        private List<FactionDto> _all = new();
        public FactionCache(GameDBContext db)
        {
            _db = db;
        }
        public IReadOnlyList<FactionDto> GetAll() => _all;
        public FactionDto? GetById(int id) => _byId.TryGetValue(id, out var dto) ? dto : null;
        public FactionDto? GetByKey(string key) => _byKey.TryGetValue(key, out var dto) ? dto : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            var entities = await _db.Factions.AsNoTracking().ToListAsync(ct);

            var list = entities.Select(FactionDto.From).ToList();

            _all = list;
            _byId = list.ToDictionary(x => x.FactionId);
            _byKey = list.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        }
    }
}
