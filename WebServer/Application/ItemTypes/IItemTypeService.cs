

using Application.Common.Models;

namespace Application.ItemTypes
{
    public interface IItemTypeService
    {
        Task<PagedResult<ItemTypeDto>> ListAsync(ListItemTypesRequest req, CancellationToken ct);
        Task<ItemTypeDto?> GetAsync(short id, CancellationToken ct);
        Task<ItemTypeDto> CreateAsync(CreateItemTypeRequest req, CancellationToken ct);
        Task UpdateAsync(short id, UpdateItemTypeRequest req, CancellationToken ct);
        Task PatchSlotAsync(short id, PatchItemTypeSlotRequest req, CancellationToken ct);
        Task DeleteAsync(short id, CancellationToken ct);
    }
}
