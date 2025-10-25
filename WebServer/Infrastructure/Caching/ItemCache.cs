using Application.Items;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public sealed class ItemCache : IItemCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private List<ItemDto> _cache = new();

        public ItemCache(IDbContextFactory<GameDBContext> factory)
        {
            _factory = factory;
        }

        public IReadOnlyList<ItemDto> GetAll() => _cache;

        public ItemDto? GetById(long id) => _cache.FirstOrDefault(x => x.Id == id);

        public ItemDto? GetByCode(string code) =>
            _cache.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            // 1) 메인 아이템들
            var items = await db.Items
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.Description,
                    x.TypeId,
                    x.RarityId,
                    x.IconId,
                    x.PortraitId,
                    x.Stackable,
                    x.MaxStack,
                    x.BindType,
                    x.Tradable,
                    x.DurabilityMax,
                    x.Weight,
                    x.Tags,
                    x.IsActive,
                    x.Meta,        // jsonb → JsonDocument
                    x.CreatedBy,
                    x.CreatedAt,
                    x.UpdatedAt,
                    x.EquipType
                })
                .ToListAsync(ct);

            var ids = items.Select(i => i.Id).ToArray();

            // 2) 서브 테이블들 한 번씩 읽고 메모리에서 묶기 (N+1 회피)
            var stats = await db.ItemStats
                .AsNoTracking()
                .Where(s => ids.Contains(s.ItemId))
                .Select(s => new { s.Id, s.ItemId, s.StatId, s.Value })
                .ToListAsync(ct);

            var effects = await db.ItemEffects
                .AsNoTracking()
                .Where(e => ids.Contains(e.ItemId))
                .Select(e => new { e.Id, e.ItemId, e.Scope, e.Payload, e.SortOrder })
                .ToListAsync(ct);

            var prices = await db.ItemPrices
                .AsNoTracking()
                .Where(p => ids.Contains(p.ItemId))
                .Select(p => new { p.Id, p.ItemId, p.CurrencyId, p.PriceType, p.Price })
                .ToListAsync(ct);

            var statsByItem = stats.GroupBy(s => s.ItemId)
                                     .ToDictionary(g => g.Key,
                                                   g => (IReadOnlyList<ItemStatDto>)g
                                                        .Select(x => new ItemStatDto(x.Id, x.StatId, x.Value))
                                                        .ToList());

            var effectsByItem = effects.GroupBy(e => e.ItemId)
                                       .ToDictionary(g => g.Key,
                                                     g => (IReadOnlyList<ItemEffectDto>)g
                                                          .OrderBy(x => x.SortOrder)
                                                          .Select(x => new ItemEffectDto(x.Id, x.Scope, x.Payload, x.SortOrder))
                                                          .ToList());

            var pricesByItem = prices.GroupBy(p => p.ItemId)
                                      .ToDictionary(g => g.Key,
                                                    g => (IReadOnlyList<ItemPriceDto>)g
                                                         .Select(x => new ItemPriceDto(x.Id, x.CurrencyId, x.PriceType, x.Price))
                                                         .ToList());

            // 3) 최종 DTO 조립 (리스트 교체)
            _cache = items.Select(i => new ItemDto(
                    i.Id,
                    i.Code,
                    i.Name,
                    i.Description ?? string.Empty,
                    i.TypeId,
                    i.RarityId,
                    i.IconId,
                    i.PortraitId,
                    i.Stackable,
                    i.MaxStack,
                    i.BindType,
                    i.Tradable,
                    i.DurabilityMax,
                    i.Weight,
                    i.Tags ?? Array.Empty<string>(),
                    i.IsActive,
                    i.Meta,               // JsonDocument (읽기 전용)
                    i.CreatedBy,
                    i.CreatedAt,
                    i.UpdatedAt,
                    statsByItem.GetValueOrDefault(i.Id) ?? Array.Empty<ItemStatDto>(),
                    effectsByItem.GetValueOrDefault(i.Id) ?? Array.Empty<ItemEffectDto>(),
                    pricesByItem.GetValueOrDefault(i.Id) ?? Array.Empty<ItemPriceDto>(),
                    i.EquipType
                ))
                .ToList();

            Console.WriteLine($"[ItemCache] loaded: {_cache.Count}");
        }
    }
}
