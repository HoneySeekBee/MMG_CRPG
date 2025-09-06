using Application.Common.Models;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Items
{
    public interface IItemService
    {
        Task<ItemDto?> GetAsync(long id, CancellationToken ct);
        Task<PagedResult<ItemDto>> ListAsync(ListItemsRequest req, CancellationToken ct);

        Task<ItemDto> CreateAsync(CreateItemRequest req, CancellationToken ct);
        Task<ItemDto> UpdateAsync(UpdateItemRequest req, CancellationToken ct);
        Task DeleteAsync(long id, CancellationToken ct);

        // 하위 엔티티 조작
        Task<ItemDto> UpsertStatAsync(long itemId, UpsertStatRequest req, CancellationToken ct);
        Task<ItemDto> RemoveStatAsync(long itemId, int statId, CancellationToken ct);

        Task<ItemDto> AddEffectAsync(long itemId, AddEffectRequest req, CancellationToken ct);
        Task<ItemDto> UpdateEffectAsync(UpdateEffectRequest req, CancellationToken ct);
        Task<ItemDto> RemoveEffectAsync(long itemId, long effectId, CancellationToken ct);

        Task<ItemDto> SetPriceAsync(long itemId, SetPriceRequest req, CancellationToken ct);
        Task<ItemDto> RemovePriceAsync(long itemId, int currencyId, Domain.Enum.ItemPriceType priceType, CancellationToken ct);
    }
}
