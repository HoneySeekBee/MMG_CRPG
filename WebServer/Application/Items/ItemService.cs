using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Items
{
    public sealed class ItemService : IItemService
    {
        private readonly IItemRepository _repo;

        public ItemService(IItemRepository repo)
        {
            _repo = repo;
        }

        // -------- 조회 --------

        public async Task<ItemDto?> GetAsync(long id, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(id, includeChildren: true, ct);
            return item is null ? null : Map(item);
        }
        public async Task<Common.Models.PagedResult<ItemDto>> ListAsync(ListItemsRequest req, CancellationToken ct)
        {
            var page = Math.Max(1, req.Page);
            var size = Math.Clamp(req.PageSize, 1, 500);

            // class 이므로 with 대신 새로 복사
            var fixedReq = new ListItemsRequest
            {
                TypeId = req.TypeId,
                RarityId = req.RarityId,
                IsActive = req.IsActive,
                Search = req.Search,
                Tags = req.Tags,
                Sort = req.Sort,
                Page = page,
                PageSize = size
            };

            // 튜플을 먼저 변수에 담아서 사용
            var result = await _repo.SearchAsync(fixedReq, ct);
            var items = result.Items;
            var total = result.TotalCount;

            var dtos = items.Select(Map).ToList();

            // 모호성 방지: 공용 PagedResult<T>만 사용 (using Application.Common.Models;)
            return new Common.Models.PagedResult<ItemDto>(dtos, page, size, total);
        }

        // -------- 생성/수정/삭제 --------

        public async Task<ItemDto> CreateAsync(CreateItemRequest req, CancellationToken ct)
        {
            // Code 유일성
            if (!await _repo.IsCodeUniqueAsync(req.Code.Trim(), excludeId: null, ct))
                throw new InvalidOperationException($"Code '{req.Code}' already exists.");

            var item = new Item(
                id: 0, // db가 채움
                code: req.Code,
                name: req.Name,
                typeId: req.TypeId,
                rarityId: req.RarityId,
                description: req.Description,
                iconId: req.IconId,
                portraitId: req.PortraitId,
                stackable: req.Stackable,
                maxStack: req.MaxStack,
                bindType: req.BindType,
                tradable: req.Tradable,
                durabilityMax: req.DurabilityMax,
                weight: req.Weight,
                tags: req.Tags,
                isActive: req.IsActive,
                meta: req.Meta,
                createdBy: req.CreatedBy
            );

            // 하위 구성 (있을 때만)
            if (req.Stats is { Count: > 0 })
                foreach (var s in req.Stats)
                    item.AddOrUpdateStat(s.StatId, s.Value);

            if (req.Effects is { Count: > 0 })
                foreach (var e in req.Effects)
                    item.AddEffect(e.Scope, e.Payload, e.SortOrder);

            if (req.Prices is { Count: > 0 })
                foreach (var p in req.Prices)
                    item.SetPrice(p.CurrencyId, p.PriceType, p.Price);

            await _repo.AddAsync(item, ct);
            await _repo.SaveChangesAsync(ct);

            // 다시 읽어와서 반환(생성된 PK/정렬 반영)
            var created = await _repo.GetByCodeAsync(req.Code, includeChildren: true, ct)
                          ?? throw new InvalidOperationException("Failed to load created item.");
            return Map(created);
        }

        public async Task<ItemDto> UpdateAsync(UpdateItemRequest req, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(req.Id, includeChildren: true, ct)
                       ?? throw new KeyNotFoundException($"Item {req.Id} not found.");

            if (req.Code is not null && !req.Code.Equals(item.Code, StringComparison.OrdinalIgnoreCase))
            {
                if (!await _repo.IsCodeUniqueAsync(req.Code.Trim(), excludeId: item.Id, ct))
                    throw new InvalidOperationException($"Code '{req.Code}' already exists.");
                // code 변경은 허용하되, 외부 연계가 있다면 주의
                item = new Item(
                    id: item.Id,
                    code: req.Code,
                    name: req.Name ?? item.Name,
                    typeId: req.TypeId ?? item.TypeId,
                    rarityId: req.RarityId ?? item.RarityId,
                    description: req.Description ?? item.Description,
                    iconId: req.IconId ?? item.IconId,
                    portraitId: req.PortraitId ?? item.PortraitId,
                    stackable: req.Stackable ?? item.Stackable,
                    maxStack: req.MaxStack ?? item.MaxStack,
                    bindType: req.BindType ?? item.BindType,
                    tradable: req.Tradable ?? item.Tradable,
                    durabilityMax: req.DurabilityMax ?? item.DurabilityMax,
                    weight: req.Weight ?? item.Weight,
                    tags: req.Tags ?? item.Tags,
                    isActive: req.IsActive ?? item.IsActive,
                    meta: req.Meta ?? item.Meta,
                    createdBy: item.CreatedBy,
                    createdAt: item.CreatedAt
                );
            }
            else
            {
                if (req.Name is not null) item.Rename(req.Name);
                if (req.Description is not null) item.ChangeDescription(req.Description);
                if (req.TypeId is not null) item = ReplaceField(item, typeId: req.TypeId.Value);
                if (req.RarityId is not null) item = ReplaceField(item, rarityId: req.RarityId.Value);
                if (req.IconId is not null) item = ReplaceField(item, iconId: req.IconId);
                if (req.PortraitId is not null) item = ReplaceField(item, portraitId: req.PortraitId);
                if (req.Stackable is not null)
                    item.ConfigureStack(req.Stackable.Value, req.MaxStack ?? item.MaxStack);
                else if (req.MaxStack is not null)
                    item.ConfigureStack(item.Stackable, req.MaxStack.Value);

                if (req.BindType is not null) item.SetBindType(req.BindType.Value);
                if (req.Tradable is not null) item.SetTradable(req.Tradable.Value);
                if (req.DurabilityMax is not null) item.SetDurabilityMax(req.DurabilityMax);
                if (req.Weight is not null) item.SetWeight(req.Weight.Value);
                if (req.Tags is not null) item.SetTags(req.Tags);
                if (req.IsActive is not null) { if (req.IsActive.Value) item.Activate(); else item.Deactivate(); }
                if (req.Meta is not null) item.SetMeta(req.Meta);
            }

            await _repo.SaveChangesAsync(ct);
            var updated = await _repo.GetByIdAsync(item.Id, includeChildren: true, ct)
                          ?? throw new InvalidOperationException("Failed to load updated item.");
            return Map(updated);

            // 로컬 헬퍼: 일부 필드만 바꾸는 복사 생성
            static Item ReplaceField(Item src,
                int? typeId = null, int? rarityId = null, int? iconId = null, int? portraitId = null)
            {
                return new Item(
                    id: src.Id, code: src.Code, name: src.Name,
                    typeId: typeId ?? src.TypeId, rarityId: rarityId ?? src.RarityId,
                    description: src.Description, iconId: iconId ?? src.IconId, portraitId: portraitId ?? src.PortraitId,
                    stackable: src.Stackable, maxStack: src.MaxStack, bindType: src.BindType, tradable: src.Tradable,
                    durabilityMax: src.DurabilityMax, weight: src.Weight, tags: src.Tags, isActive: src.IsActive,
                    meta: src.Meta, createdBy: src.CreatedBy, createdAt: src.CreatedAt);
            }
        }

        public async Task DeleteAsync(long id, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(id, includeChildren: false, ct)
                       ?? throw new KeyNotFoundException($"Item {id} not found.");
            await _repo.DeleteAsync(item, ct);
            await _repo.SaveChangesAsync(ct);
        }

        // -------- 하위 엔티티 --------

        public async Task<ItemDto> UpsertStatAsync(long itemId, UpsertStatRequest req, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(itemId, includeChildren: true, ct)
                       ?? throw new KeyNotFoundException($"Item {itemId} not found.");

            item.AddOrUpdateStat(req.StatId, req.Value);
            await _repo.SaveChangesAsync(ct);

            var reloaded = await _repo.GetByIdAsync(item.Id, includeChildren: true, ct);
            return Map(reloaded!);
        }

        public async Task<ItemDto> RemoveStatAsync(long itemId, int statId, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(itemId, includeChildren: true, ct)
                       ?? throw new KeyNotFoundException($"Item {itemId} not found.");
            item.RemoveStat(statId);
            await _repo.SaveChangesAsync(ct);
            var reloaded = await _repo.GetByIdAsync(item.Id, includeChildren: true, ct);
            return Map(reloaded!);
        }

        public async Task<ItemDto> AddEffectAsync(long itemId, AddEffectRequest req, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(itemId, includeChildren: true, ct)
                       ?? throw new KeyNotFoundException($"Item {itemId} not found.");
            item.AddEffect(req.Scope, req.Payload, req.SortOrder);
            await _repo.SaveChangesAsync(ct);
            var reloaded = await _repo.GetByIdAsync(item.Id, includeChildren: true, ct);
            return Map(reloaded!);
        }
        public async Task<ItemDto> UpdateEffectAsync(UpdateEffectRequest req, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(req.ItemId, includeChildren: true, ct)
                       ?? throw new KeyNotFoundException($"Item {req.ItemId} not found.");

            var effect = item.Effects.FirstOrDefault(e => e.Id == req.EffectId)
                         ?? throw new KeyNotFoundException($"Effect {req.EffectId} not found.");

            if (req.Scope is not null)
            {
                // 범위가 바뀌면 삭제 후 다시 추가 (Id는 새로 부여됨)
                var payload = req.Payload ?? effect.Payload;   // JsonDocument
                var sort = req.SortOrder ?? effect.SortOrder;

                item.RemoveEffect(effect.Id);
                item.AddEffect(req.Scope.Value, payload, sort);
            }
            else
            {
                if (req.Payload is not null)
                    effect.ReplacePayload(req.Payload);        // JsonDocument
                if (req.SortOrder is not null)
                    effect.SetSortOrder(req.SortOrder.Value);
            }

            await _repo.SaveChangesAsync(ct);

            var reloaded = await _repo.GetByIdAsync(item.Id, includeChildren: true, ct)
                          ?? throw new InvalidOperationException("Failed to reload item.");

            return Map(reloaded);
        }

        public async Task<ItemDto> RemoveEffectAsync(long itemId, long effectId, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(itemId, includeChildren: true, ct)
                       ?? throw new KeyNotFoundException($"Item {itemId} not found.");
            item.RemoveEffect(effectId);
            await _repo.SaveChangesAsync(ct);
            var reloaded = await _repo.GetByIdAsync(item.Id, includeChildren: true, ct);
            return Map(reloaded!);
        }

        public async Task<ItemDto> SetPriceAsync(long itemId, SetPriceRequest req, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(itemId, includeChildren: true, ct)
                       ?? throw new KeyNotFoundException($"Item {itemId} not found.");
            item.SetPrice(req.CurrencyId, req.PriceType, req.Price);
            await _repo.SaveChangesAsync(ct);
            var reloaded = await _repo.GetByIdAsync(item.Id, includeChildren: true, ct);
            return Map(reloaded!);
        }

        public async Task<ItemDto> RemovePriceAsync(long itemId, int currencyId, ItemPriceType priceType, CancellationToken ct)
        {
            var item = await _repo.GetByIdAsync(itemId, includeChildren: true, ct)
                       ?? throw new KeyNotFoundException($"Item {itemId} not found.");
            item.RemovePrice(currencyId, priceType);
            await _repo.SaveChangesAsync(ct);
            var reloaded = await _repo.GetByIdAsync(item.Id, includeChildren: true, ct);
            return Map(reloaded!);
        }

        // -------- 매핑 --------

        private static ItemDto Map(Item i)
        {
            return new ItemDto(
                i.Id, i.Code, i.Name, i.Description, i.TypeId, i.RarityId, i.IconId, i.PortraitId,
                i.Stackable, i.MaxStack, i.BindType, i.Tradable, i.DurabilityMax, i.Weight,
                i.Tags, i.IsActive, i.Meta, i.CreatedBy, i.CreatedAt, i.UpdatedAt,
                i.Stats.Select(s => new ItemStatDto(s.Id, s.StatId, s.Value)).ToList(),
                i.Effects.Select(e => new ItemEffectDto(e.Id, e.Scope, e.Payload, e.SortOrder)).ToList(),
                i.Prices.Select(p => new ItemPriceDto(p.Id, p.CurrencyId, p.PriceType, p.Price)).ToList()
            );
        }
    }
}
