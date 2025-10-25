using Application.EquipSlots;
using Contracts.EquipSlots;

namespace WebServer.Mappers
{
    public static class EquipSlotsProtoMapper
    {
        public static EquipSlotPb ToProto(this EquipSlotDto d) => new EquipSlotPb
        {
            Id = d.Id, 
            Code = d.Code,
            Name = d.Name,
            SortOrder = d.SortOrder,
            IconId = d.IconId, 
            UpdatedAt = d.UpdatedAt.ToUnixTimeMilliseconds()
        };
         
        public static EquipSlotListPb ToProtoList(this IEnumerable<EquipSlotDto> src)
        {
            var list = new EquipSlotListPb();
            list.EquipSlots.AddRange(src.Select(ToProto));
            return list;
        }
    }
}
