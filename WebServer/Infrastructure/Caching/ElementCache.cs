using Application.Elements;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public sealed class ElementCache : IElementCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private readonly object _gate = new();

        private List<ElementDto> _all = new();
        private Dictionary<int, ElementDto> _byId = new();
        private Dictionary<string, ElementDto> _byKey = new(StringComparer.OrdinalIgnoreCase);

        public ElementCache(IDbContextFactory<GameDBContext> factory) => _factory = factory;

        public IReadOnlyList<ElementDto> GetAll() => _all;
        public ElementDto? GetById(int id) => _byId.TryGetValue(id, out var v) ? v : null;
        public ElementDto? GetByKey(string key) => _byKey.TryGetValue(key, out var v) ? v : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            var list = await db.Elements.AsNoTracking()
                .OrderBy(x => x.SortOrder).ThenBy(x => x.ElementId)
                .Select(e => new ElementDto(
                    e.ElementId,
                    e.Key,
                    e.Label,
                    e.IconId,
                    e.ColorHex,
                    e.SortOrder,
                    e.IsActive,
                    e.Meta ?? string.Empty,
                    e.CreatedAt,
                    e.UpdatedAt
                ))
                .ToListAsync(ct);

            var byId = list.ToDictionary(x => x.ElementId);
            var byKey = list.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

            lock (_gate)
            {
                _all = list;
                _byId = byId;
                _byKey = byKey;
            }

            Console.WriteLine($"[ElementCache] loaded: {_all.Count}");
        }
    }
}
