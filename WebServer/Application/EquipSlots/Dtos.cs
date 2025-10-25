using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace Application.EquipSlots
{
    public sealed record EquipSlotDto(short Id, string Code, string Name, short SortOrder, int IconId, DateTimeOffset UpdatedAt);

    public sealed record EquipSlotCreateDto(string Code, string Name, short SortOrder, int IconId);
    public sealed record EquipSlotUpdateDto(string Code, string Name, short SortOrder, int IconId); 
    public sealed record EquipSlotCacheItem(short id, string Code, string Name, short SortOrder, int IconId, long Version);

    public static class EquipSlotMappings
    {
        public static EquipSlotDto ToDto(this EquipSlot e) =>
            new(e.Id, e.Code, e.Name, e.SortOrder, e.IconId, e.UpdatedAt);

        public static EquipSlotCacheItem ToCacheItem(this EquipSlot e) =>
            new(e.Id, e.Code, e.Name, e.SortOrder, e.IconId, e.UpdatedAt.Ticks);
    }

}
