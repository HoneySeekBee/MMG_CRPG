using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ItemTypes
{
    public sealed class ItemTypeService : IItemTypeService
    {
        private readonly IItemTypeRepository _repo;
        public ItemTypeService(IItemTypeRepository repo) => _repo = repo;

        public async Task<Common.Models.PagedResult<ItemTypeDto>> ListAsync(ListItemTypesRequest req, CancellationToken ct)
        {
            var page = Math.Max(1, req.Page);
            var size = Math.Clamp(req.PageSize, 1, 500);

            var (items, total) = await _repo.SearchAsync(req with { Page = page, PageSize = size }, ct);
            var dtos = items.Select(Map).ToList();
            return new Common.Models.PagedResult<ItemTypeDto>(dtos, page, size, total);
        }

        public async Task<ItemTypeDto?> GetAsync(short id, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, includeSlot: true, ct);
            return e is null ? null : Map(e);
        }

        public async Task<ItemTypeDto> CreateAsync(CreateItemTypeRequest req, CancellationToken ct)
        {
            var entity = new ItemType(req.Code, req.Name, req.SlotId);
            await _repo.AddAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);
            var created = await _repo.GetByIdAsync(entity.Id, includeSlot: true, ct);
            return Map(created!);
        }
        public async Task UpdateAsync(short id, UpdateItemTypeRequest req, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct: ct)
                    ?? throw new KeyNotFoundException($"ItemType {id} not found");
            e.ChangeCode(req.Code);
            e.Rename(req.Name);
            e.SetSlot(req.SlotId);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task PatchSlotAsync(short id, PatchItemTypeSlotRequest req, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct: ct)
                    ?? throw new KeyNotFoundException($"ItemType {id} not found");
            e.SetSlot(req.SlotId);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(short id, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct: ct)
                    ?? throw new KeyNotFoundException($"ItemType {id} not found");
            await _repo.RemoveAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
        }
        private static ItemTypeDto Map(ItemType x) =>
            new(x.Id, x.Code, x.Name, x.SlotId, x.Slot?.Code, x.Slot?.Name, x.CreatedAt, x.UpdatedAt);
    }
}
