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
        private readonly GameDBContext _db;
        private Dictionary<int, ElementDto> _byId = new();
        private Dictionary<string, ElementDto> _byKey = new();
        private List<ElementDto> _all = new();

        public ElementCache(GameDBContext db)
        {
            _db = db;
        }

        public IReadOnlyList<ElementDto> GetAll() => _all;
        public ElementDto? GetById(int id) => _byId.TryGetValue(id, out var dto) ? dto : null;
        public ElementDto? GetByKey(string key) => _byKey.TryGetValue(key, out var dto) ? dto : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            var entities = await _db.Elements.AsNoTracking().ToListAsync(ct);

            var list = entities.Select(e => new ElementDto(
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
            )).ToList();

            _all = list;
            _byId = list.ToDictionary(x => x.ElementId);
            _byKey = list.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        }
    }
}
