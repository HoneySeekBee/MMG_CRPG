using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Item
    {
        // [PK & 기본 정보]
        public long Id { get; private set; }                       // bigserial
        public string Code { get; private set; }                   // UNIQUE, 외부노출용 안전키
        public string Name { get; private set; }                   // 다국어 미사용 가정
        public string Description { get; private set; } = string.Empty;

        // [분류 & 리소스]
        public int TypeId { get; private set; }                    // FK -> ItemType
        public int RarityId { get; private set; }                  // FK -> Rarity
        public int? IconId { get; private set; }                   // FK -> Icons
        public int? PortraitId { get; private set; }               // FK -> Portraits

        // [동작/규칙]
        public bool Stackable { get; private set; } = true;
        public int MaxStack { get; private set; } = 99;            // Stackable=false면 1
        public BindType BindType { get; private set; } = BindType.None;
        public bool Tradable { get; private set; } = true;
        public int? DurabilityMax { get; private set; }            // 장비류만 사용(null 가능)
        public int Weight { get; private set; } = 0;

        // [태그/메타/상태]
        public string[] Tags { get; private set; } = Array.Empty<string>();
        public bool IsActive { get; private set; } = true;
        public JsonDocument? Meta { get; private set; }                // 자유 확장(JSONB 대응)

        // [감사/생성시각]
        public string? CreatedBy { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
        public int? EquipType { get; private set; }

        // [하위 엔티티 컬렉션]
        private readonly List<ItemStat> _stats = new();
        private readonly List<ItemEffect> _effects = new();
        private readonly List<ItemPrice> _prices = new();

        public IReadOnlyList<ItemStat> Stats => _stats;
        public IReadOnlyList<ItemEffect> Effects => _effects;
        public IReadOnlyList<ItemPrice> Prices => _prices;

        private Item() { } // ORM용

        public Item(
            long id,
            string code,
            string name,
            int typeId,
            int rarityId,
            string? description = null,
            int? iconId = null,
            int? portraitId = null,
            bool stackable = true,
            int maxStack = 99,
            BindType bindType = BindType.None,
            bool tradable = true,
            int? durabilityMax = null,
            int weight = 0,
            IEnumerable<string>? tags = null,
            bool isActive = true,
             JsonDocument? meta = null,
            string? createdBy = null,
            DateTimeOffset? createdAt = null,
            int? equipType  = null 
        )
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

            Id = id;
            Code = code.Trim();
            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;

            TypeId = typeId;
            RarityId = rarityId;
            IconId = iconId;
            PortraitId = portraitId;

            Stackable = stackable;
            MaxStack = maxStack;
            BindType = bindType;
            Tradable = tradable;
            DurabilityMax = durabilityMax;
            Weight = weight;

            SetTags(tags);
            IsActive = isActive;
            Meta = meta;

            CreatedBy = createdBy;
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow;

            EquipType = equipType;

            Touch();

            EnforceInvariants();
        }

        // ----------------- 도메인 동작(상태 변경) -----------------

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
            Name = name.Trim();
            Touch();
        }

        public void ChangeDescription(string? description)
        {
            Description = description?.Trim() ?? string.Empty;
            Touch();
        }

        public void ConfigureStack(bool stackable, int maxStack = 99)
        {
            Stackable = stackable;
            MaxStack = stackable ? Math.Max(1, maxStack) : 1;
            EnforceInvariants();
            Touch();
        }

        public void SetDurabilityMax(int? durability)
        {
            if (durability is < 0) throw new ArgumentOutOfRangeException(nameof(durability));
            DurabilityMax = durability;
            Touch();
        }

        public void SetBindType(BindType bindType)
        {
            BindType = bindType;
            Touch();
        }

        public void SetTradable(bool tradable)
        {
            Tradable = tradable;
            Touch();
        }

        public void SetWeight(int weight)
        {
            if (weight < 0) throw new ArgumentOutOfRangeException(nameof(weight));
            Weight = weight;
            Touch();
        }

        public void SetTags(IEnumerable<string>? tags)
        {
            Tags = (tags ?? Enumerable.Empty<string>())
                .Select(t => t?.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t!.ToLowerInvariant())
                .Distinct()
                .ToArray();
            Touch();
        }

        public void Activate() { IsActive = true; Touch(); }
        public void Deactivate() { IsActive = false; Touch(); }

        public void SetMeta(JsonDocument? meta)
        {
            Meta = meta;
            Touch();
        }

        // ----- 하위 엔티티 조작 -----

        public ItemStat AddOrUpdateStat(int statId, decimal value)
        {
            var existing = _stats.FirstOrDefault(s => s.StatId == statId);
            if (existing is not null)
            {
                existing.Update(value);
                Touch();
                return existing;
            }

            var created = new ItemStat(Id, statId, value );
            _stats.Add(created);
            Touch();
            return created;
        }

        public bool RemoveStat(int statId)
        {
            var removed = _stats.RemoveAll(s => s.StatId == statId) > 0;
            if (removed) Touch();
            return removed;
        }

        public ItemEffect AddEffect(ItemEffectScope scope, JsonDocument payload, short? sortOrder = null)
        {
            var order = sortOrder ?? (short)(_effects.Count == 0 ? 0 : _effects.Max(e => e.SortOrder) + 1);
            var created = new ItemEffect(Id, scope, payload, order);
            _effects.Add(created);
            Touch();
            return created;
        }

        public void ReorderEffects(IEnumerable<(long effectId, short sortOrder)> orders)
        {
            var map = orders.ToDictionary(x => x.effectId, x => x.sortOrder);
            foreach (var e in _effects)
                if (map.TryGetValue(e.Id, out var o)) e.SetSortOrder(o);
            Touch();
        }

        public bool RemoveEffect(long effectId)
        {
            var removed = _effects.RemoveAll(e => e.Id == effectId) > 0;
            if (removed) Touch();
            return removed;
        }

        public ItemPrice SetPrice(int currencyId, ItemPriceType priceType, long price)
        {
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));

            var existing = _prices.FirstOrDefault(p => p.CurrencyId == currencyId && p.PriceType == priceType);
            if (existing is not null)
            {
                existing.Update(price);
                Touch();
                return existing;
            }

            var created = new ItemPrice(Id, currencyId, priceType, price);
            _prices.Add(created);
            Touch();
            return created;
        }

        public bool RemovePrice(int currencyId, ItemPriceType priceType)
        {
            var removed = _prices.RemoveAll(p => p.CurrencyId == currencyId && p.PriceType == priceType) > 0;
            if (removed) Touch();
            return removed;
        }

        // ----------------- 내부 규칙/유틸 -----------------

        private void EnforceInvariants()
        {
            if (!Stackable && MaxStack != 1)
                throw new InvalidOperationException("Non-stackable item must have MaxStack = 1.");

            if (Weight < 0) throw new InvalidOperationException("Weight cannot be negative.");
            if (DurabilityMax is < 0) throw new InvalidOperationException("DurabilityMax cannot be negative.");
        }

        private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
    }

    // ----------------- 하위 엔티티 -----------------

    public sealed class ItemStat
    {
        public long Id { get; private set; }           // DB bigserial (도메인에선 0으로 시작 가능)
        public long ItemId { get; private set; }
        public int StatId { get; private set; }        // FK -> StatTypes
        public decimal Value { get; private set; }     // NUMERIC(12,4) 
        public StatType StatType { get; private set; }
        private ItemStat() { }
        public ItemStat(long itemId, int statId, decimal value)
        {
            ItemId = itemId;
            StatId = statId;
            Update(value);
        }
        public ItemStat(long itemId, int statId, decimal value, StatType statType) : this(itemId, statId, value)
       => StatType = statType;

        public void Update(decimal value)
        {
            Value = value;
        }
    }

    public sealed class ItemEffect
    {
        public long Id { get; private set; }
        public long ItemId { get; private set; }
        public ItemEffectScope Scope { get; private set; }
        public JsonDocument Payload { get; private set; }
        public short SortOrder { get; private set; }

        private ItemEffect() { }

        public ItemEffect(long itemId, ItemEffectScope scope, JsonDocument payload, short sortOrder = 0)
        {
            ItemId = itemId;
            Scope = scope;
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
            SortOrder = sortOrder;
        }

        public void ReplacePayload(JsonDocument payload)
        {
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }

        public void SetSortOrder(short order) => SortOrder = order;
    }

    public sealed class ItemPrice
    {
        public long Id { get; private set; }
        public long ItemId { get; private set; }
        public int CurrencyId { get; private set; }             // FK -> Currencies
        public ItemPriceType PriceType { get; private set; }    // BUY/SELL/UPGRADE/CRAFT
        public long Price { get; private set; }                 // >= 0

        private ItemPrice() { }

        public ItemPrice(long itemId, int currencyId, ItemPriceType priceType, long price)
        {
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
            ItemId = itemId;
            CurrencyId = currencyId;
            PriceType = priceType;
            Price = price;
        }

        public void Update(long price)
        {
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
            Price = price;
        }
    }
}
