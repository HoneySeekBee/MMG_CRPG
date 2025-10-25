using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Application.Items
{
    public sealed record ItemDto(
        long Id,
        string Code,
        string Name,
        string Description,
        int TypeId,
        int RarityId,
        int? IconId,
        int? PortraitId,
        bool Stackable,
        int MaxStack,
        BindType BindType,
        bool Tradable,
        int? DurabilityMax,
        int Weight,
        string[] Tags,
        bool IsActive,
        JsonDocument? Meta,
        string? CreatedBy,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        IReadOnlyList<ItemStatDto> Stats,
        IReadOnlyList<ItemEffectDto> Effects,
        IReadOnlyList<ItemPriceDto> Prices,
        int? EquipType
    );

    public sealed record ItemStatDto(
        long Id,
        int StatId,
        decimal Value
    );

    public sealed record ItemEffectDto(
        long Id,
        ItemEffectScope Scope,
        JsonDocument Payload,
        short SortOrder
    );

    public sealed record ItemPriceDto(
        long Id,
        int CurrencyId,
        ItemPriceType PriceType,
        long Price
    );
}
