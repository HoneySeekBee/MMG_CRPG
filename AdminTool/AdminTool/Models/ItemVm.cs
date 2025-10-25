using Application.Items;
using Domain.Enum;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AdminTool.Models
{
    public sealed class ItemListFilterVm
    {
        public int? TypeId { get; set; }
        public int? RarityId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string[]? Tags { get; set; }
        public string? Sort { get; set; } = "code";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        public ListItemsRequest ToRequest() => new()
        {
            TypeId = TypeId,
            RarityId = RarityId,
            IsActive = IsActive,
            Search = Search,
            Tags = Tags,
            Sort = Sort,
            Page = Page,
            PageSize = PageSize
        };
    }

    /// <summary>목록 그리드용 요약 행</summary>
    public sealed class ItemVm
    {
        public long Id { get; init; }
        public string Code { get; init; } = "";
        public string Name { get; init; } = "";
        public int TypeId { get; init; }
        public int RarityId { get; init; }
        public bool IsActive { get; init; }
        public bool Stackable { get; init; }
        public int MaxStack { get; init; }
        public int Weight { get; init; }
        public string Tags { get; init; } = ""; // UI 표시용(콤마 구분)
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
        public int StatCount { get; init; }
        public int EffectCount { get; init; }
        public int PriceCount { get; init; }

        public static ItemVm From(ItemDto d) => new()
        {
            Id = d.Id,
            Code = d.Code,
            Name = d.Name,
            TypeId = d.TypeId,
            RarityId = d.RarityId,
            IsActive = d.IsActive,
            Stackable = d.Stackable,
            MaxStack = d.MaxStack,
            Weight = d.Weight,
            Tags = string.Join(",", d.Tags ?? Array.Empty<string>()),
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt,
            StatCount = d.Stats?.Count ?? 0,
            EffectCount = d.Effects?.Count ?? 0,
            PriceCount = d.Prices?.Count ?? 0
        };
    }

    // ───────────────────────────── Edit VM ─────────────────────────────
    public sealed class ItemEditVm
    {
        public long Id { get; set; }                    // 0 = 신규
        [Required, StringLength(100)] public string Code { get; set; } = "";
        [Required, StringLength(200)] public string Name { get; set; } = "";
        [StringLength(1000)] public string? Description { get; set; }

        [Range(1, short.MaxValue)] public int TypeId { get; set; }
        [Range(1, short.MaxValue)] public int RarityId { get; set; }
        public int? IconId { get; set; }
        public List<IconPickItem> Icons { get; set; } = new();
        public int? PortraitId { get; set; }

        public bool Stackable { get; set; } = true;
        [Range(1, 9999)] public int MaxStack { get; set; } = 99;
        public BindType BindType { get; set; } = BindType.None;
        public bool Tradable { get; set; } = true;
        [Range(0, 999999)] public int? DurabilityMax { get; set; }
        [Range(0, 999999)] public int Weight { get; set; } = 0;

        [Range(1, short.MaxValue)]
        public int? EquipType { get; set; }
        /// <summary>UI에서는 콤마/공백 구분 문자열 입력</summary>
        public string Tags { get; set; } = "";
        public bool IsActive { get; set; } = true;

        /// <summary>JSON 입력란(문자열) – 저장 시 JsonDocument로 변환</summary>
        public string? MetaJson { get; set; }

        // 하위 컬렉션 (간단 편집용)
        public List<ItemStatVm> Stats { get; set; } = new();
        public List<ItemEffectVm> Effects { get; set; } = new();
        public List<ItemPriceVm> Prices { get; set; } = new();

        // ------- 매핑 -------
        public static ItemEditVm From(ItemDto d) => new()
        {
            Id = d.Id,
            Code = d.Code,
            Name = d.Name,
            Description = d.Description,
            TypeId = d.TypeId,
            RarityId = d.RarityId,
            IconId = d.IconId,
            PortraitId = d.PortraitId,
            Stackable = d.Stackable,
            MaxStack = d.MaxStack,
            BindType = d.BindType,
            Tradable = d.Tradable,
            DurabilityMax = d.DurabilityMax,
            Weight = d.Weight,
            Tags = string.Join(", ", d.Tags ?? Array.Empty<string>()),
            IsActive = d.IsActive,
            MetaJson = d.Meta?.RootElement.GetRawText(),
            Stats = (d.Stats ?? Array.Empty<ItemStatDto>()).Select(ItemStatVm.From).ToList(),
            Effects = (d.Effects ?? Array.Empty<ItemEffectDto>()).Select(ItemEffectVm.From).ToList(),
            Prices = (d.Prices ?? Array.Empty<ItemPriceDto>()).Select(ItemPriceVm.From).ToList(),
            EquipType = d.EquipType
        };

        public CreateItemRequest ToCreateRequest(string createdBy)
        {
            return new CreateItemRequest
            {
                Code = Code.Trim(),
                Name = Name.Trim(),
                Description = Description ?? "",
                TypeId = TypeId,
                RarityId = RarityId,
                IconId = IconId,
                PortraitId = PortraitId,
                Stackable = Stackable,
                MaxStack = Stackable ? Math.Max(1, MaxStack) : 1,
                BindType = BindType,
                Tradable = Tradable,
                DurabilityMax = DurabilityMax,
                Weight = Weight,
                Tags = ParseTags(Tags),
                IsActive = IsActive,
                Meta = TryParseJson(MetaJson),
                CreatedBy = createdBy,
                Stats = Stats.Select(s => s.ToRequest()).ToList(),
                Effects = Effects.Select(e => e.ToAddRequest()).ToList(),
                Prices = Prices.Select(p => p.ToRequest()).ToList(),
                EquipType = (TypeId == 17 ? EquipType : null),
            };
        }

        public UpdateItemRequest ToUpdateRequest()
        {
            return new UpdateItemRequest
            {
                Id = Id,
                Code = Code.Trim(),
                Name = Name.Trim(),
                Description = Description ?? "",
                TypeId = TypeId,
                RarityId = RarityId,
                IconId = IconId,
                PortraitId = PortraitId,
                Stackable = Stackable,
                MaxStack = Stackable ? Math.Max(1, MaxStack) : 1,
                BindType = BindType,
                Tradable = Tradable,
                DurabilityMax = DurabilityMax,
                Weight = Weight,
                Tags = ParseTags(Tags),
                IsActive = IsActive,
                Meta = TryParseJson(MetaJson),
                EquipType = (TypeId == 17 ? EquipType : null)
            };
        }

        // Helpers
        public static string[] ParseTags(string? s) =>
            (s ?? "")
            .Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray();

        public static JsonDocument? TryParseJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonDocument.Parse(json); }
            catch { return null; }
        }
    }

    // ───────────────────────────── Child VMs ─────────────────────────────
    public sealed class ItemStatVm
    {
        public long Id { get; set; }
        [Range(1, short.MaxValue)] public int StatId { get; set; }
        [Range(typeof(decimal), "0", "9999999999")] public decimal Value { get; set; }

        public static ItemStatVm From(ItemStatDto d) => new() { Id = d.Id, StatId = d.StatId, Value = d.Value };
        public UpsertStatRequest ToRequest() => new() { StatId = StatId, Value = Value };
    }

    public sealed class ItemEffectVm
    {
        public long Id { get; set; }
        public ItemEffectScope Scope { get; set; } = ItemEffectScope.OnUse;
        /// <summary>JSON 문자열 편집란</summary>
        public string PayloadJson { get; set; } = "{}";
        public short SortOrder { get; set; } = 0;

        public static ItemEffectVm From(ItemEffectDto d) => new()
        {
            Id = d.Id,
            Scope = d.Scope,
            PayloadJson = d.Payload?.RootElement.GetRawText() ?? "{}",
            SortOrder = d.SortOrder
        };

        public AddEffectRequest ToAddRequest() => new()
        {
            Scope = Scope,
            Payload = ItemEditVm.TryParseJson(PayloadJson)!,
            SortOrder = SortOrder
        };

        public UpdateEffectRequest ToUpdateRequest(long itemId) => new()
        {
            ItemId = itemId,
            EffectId = Id,
            Scope = Scope,
            Payload = ItemEditVm.TryParseJson(PayloadJson),
            SortOrder = SortOrder
        };
    }

    public sealed class ItemPriceVm
    {
        public long Id { get; set; }
        [Range(1, short.MaxValue)] public int CurrencyId { get; set; } = 1;
        public ItemPriceType PriceType { get; set; } = ItemPriceType.Buy;
        [Range(0, long.MaxValue)] public long Price { get; set; } = 0;

        public static ItemPriceVm From(ItemPriceDto d) => new()
        { Id = d.Id, CurrencyId = d.CurrencyId, PriceType = d.PriceType, Price = d.Price };

        public SetPriceRequest ToRequest() => new()
        { CurrencyId = CurrencyId, PriceType = PriceType, Price = Price };
    }
}