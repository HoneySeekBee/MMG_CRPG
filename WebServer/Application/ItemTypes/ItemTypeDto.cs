using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ItemTypes
{
    public sealed record ItemTypeDto(
        short Id,
        string Code,
        string Name,
        short? SlotId,
        string? SlotCode,
        string? SlotName,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        bool Active
    );

    public sealed record ListItemTypesRequest(
        string? Search = null,
        bool? HasSlot = null,           // true: SlotId not null / false: null
        string Sort = "code",           // code|name|slot|created|updated
        bool Desc = false,
        int Page = 1,
        int PageSize = 50
    );

    public sealed record CreateItemTypeRequest(
        string Code,
        string Name,
        short? SlotId,
        bool Active
    );

    public sealed record UpdateItemTypeRequest(
        short Id,
        string Code,
        string Name,
        short? SlotId,
        bool Active
    );

    public sealed record PatchItemTypeSlotRequest(
        short Id,
        short? SlotId
    );
}
