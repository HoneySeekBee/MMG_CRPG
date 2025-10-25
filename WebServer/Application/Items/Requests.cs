using Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Application.Items
{
    public sealed class ListItemsRequest
    {
        public int? TypeId { get; init; }
        public int? RarityId { get; init; }
        public bool? IsActive { get; init; }
        public string? Search { get; init; }          // Code/Name 부분일치
        public string[]? Tags { get; init; }          // ANY 매칭
        public string? Sort { get; init; } = "rarity,type,code"; // 서버 기본 정렬 키
        [Range(1, 500)] public int PageSize { get; init; } = 50;
        [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    }
    public sealed class CreateItemRequest
    {
        // 기본
        [Required, MinLength(1)] public string Code { get; init; } = default!;
        [Required, MinLength(1)] public string Name { get; init; } = default!;
        public string? Description { get; init; }

        [Required] public int TypeId { get; init; }
        [Required] public int RarityId { get; init; }
        public int? IconId { get; init; }
        public int? PortraitId { get; init; }

        // 규칙
        public bool Stackable { get; init; } = true;
        [Range(1, int.MaxValue)] public int MaxStack { get; init; } = 99;
        public BindType BindType { get; init; } = BindType.None;
        public bool Tradable { get; init; } = true;
        [Range(0, int.MaxValue)] public int? DurabilityMax { get; init; }
        [Range(0, int.MaxValue)] public int Weight { get; init; } = 0;

        public int? EquipType { get; set; }
        // 상태/메타
        public string[]? Tags { get; init; }
        public bool IsActive { get; init; } = true;
        public JsonDocument? Meta { get; init; }
        public string? CreatedBy { get; init; }

        // 하위 집합(옵션)
        public IReadOnlyList<UpsertStatRequest>? Stats { get; init; }
        public IReadOnlyList<AddEffectRequest>? Effects { get; init; }
        public IReadOnlyList<SetPriceRequest>? Prices { get; init; }
    }

    // ---------- 수정(전체/부분) ----------
    /// <summary>부분 수정 패턴. null은 "변경 안 함"</summary>
    public sealed class UpdateItemRequest
    {
        [Required] public long Id { get; init; }

        public string? Code { get; init; }
        public string? Name { get; init; }
        public string? Description { get; init; }
        public int? TypeId { get; init; }
        public int? RarityId { get; init; }
        public int? IconId { get; init; }
        public int? PortraitId { get; init; }

        public int? EquipType { get; set; }
        public bool? Stackable { get; init; }
        [Range(1, int.MaxValue)] public int? MaxStack { get; init; }
        public BindType? BindType { get; init; }
        public bool? Tradable { get; init; }
        [Range(0, int.MaxValue)] public int? DurabilityMax { get; init; }
        [Range(0, int.MaxValue)] public int? Weight { get; init; }

        public string[]? Tags { get; init; }
        public bool? IsActive { get; init; }
        public JsonDocument? Meta { get; init; }
    }

    // ---------- 하위 엔티티 조작 ----------
    public sealed class UpsertStatRequest
    {
        [Required] public int StatId { get; init; }
        [Required] public decimal Value { get; init; }
    }

    public sealed class RemoveStatRequest
    {
        [Required] public long ItemId { get; init; }
        [Required] public int StatId { get; init; }
    }

    public sealed class AddEffectRequest
    {
        [Required] public ItemEffectScope Scope { get; init; }
        [Required] public JsonDocument Payload { get; init; } = default!;
        public short? SortOrder { get; init; }
    }

    public sealed class UpdateEffectRequest
    {
        [Required] public long ItemId { get; init; }
        [Required] public long EffectId { get; init; }
        public ItemEffectScope? Scope { get; init; }
        public JsonDocument? Payload { get; init; }
        public short? SortOrder { get; init; }
    }

    public sealed class RemoveEffectRequest
    {
        [Required] public long ItemId { get; init; }
        [Required] public long EffectId { get; init; }
    }

    public sealed class SetPriceRequest
    {
        [Required] public int CurrencyId { get; init; }
        [Required] public ItemPriceType PriceType { get; init; } = ItemPriceType.Buy;
        [Range(0, long.MaxValue)] public long Price { get; init; }
    }

    public sealed class RemovePriceRequest
    {
        [Required] public long ItemId { get; init; }
        [Required] public int CurrencyId { get; init; }
        [Required] public ItemPriceType PriceType { get; init; }
    }
}
