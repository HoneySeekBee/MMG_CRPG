using Application.EquipSlots;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public class EquipSlotCache : IEquipSlotCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private readonly SemaphoreSlim _gate = new(1, 1);

        private ImmutableArray<EquipSlotDto> _all = ImmutableArray<EquipSlotDto>.Empty;
        
        private ConcurrentDictionary<int, EquipSlotDto> _byId = new();
        public EquipSlotCache(IDbContextFactory<GameDBContext> factory)
          => _factory = factory;
        public IReadOnlyList<EquipSlotDto> GetAll() => _all;
        public EquipSlotDto? GetById(int id)
            => _byId.TryGetValue(id, out var dto) ? dto : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await _gate.WaitAsync(ct);
            try
            {
                await using var db = await _factory.CreateDbContextAsync(ct);

                // 전체 로드 (정렬: SortOrder → Id)
                var list = await db.EquipSlots
                    .AsNoTracking()
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Id)
                    .Select(x => new EquipSlotDto(
                        x.Id,        // short
                        x.Code,
                        x.Name,
                        x.SortOrder,
                        x.IconId,
                        x.UpdatedAt
                    ))
                    .ToListAsync(ct);

                // 새 스냅샷/인덱스 빌드
                var newAll = list.ToImmutableArray();

                var newById = new ConcurrentDictionary<int, EquipSlotDto>(
                    list.Select(e => new KeyValuePair<int, EquipSlotDto>(e.Id, e)));

                Console.WriteLine($"[ CACHE ]  EquipSlot : {newAll.Count()} 개 ");
                // 원자적 교체
                _all = newAll;
                _byId = newById;
            }
            finally
            {
                _gate.Release();
            }
        }

    }
}
