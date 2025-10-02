using Application.ItemTypes;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public class ItemTypeCache : IItemTypeCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory; 
        private List<ItemTypeDto> _cache = new();

        public ItemTypeCache(IDbContextFactory<GameDBContext> factory )
        {
            _factory = factory;
        }
        public IReadOnlyList<ItemTypeDto> GetAll() => _cache;
        public ItemTypeDto? GetById(short id) => _cache.FirstOrDefault(x => x.Id == id);
        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            _cache = await db.ItemTypes
                .Select(x => new ItemTypeDto(
                    x.Id, x.Code, x.Name, x.SlotId,
                    x.Slot != null ? x.Slot.Code : null,
                    x.Slot != null ? x.Slot.Name : null,
                    x.CreatedAt, x.UpdatedAt, x.Active))
                .ToListAsync(ct);

            Console.WriteLine($"[ItemTypeCache] loaded: {_cache.Count}"); 
        }
    }
}
