 
using Application.UserCharacterEquips;
using Contracts.Protos;
using Google.Protobuf.WellKnownTypes;

namespace WebServer.Mappers
{
    public static class CharacterEquipmentMappers
    {
        public static GetCharacterEquipmentResponse ToPbGet(this UserCharacterEquipSnapshotDto dto)
        {
            var resp = new GetCharacterEquipmentResponse
            {
                UserId = dto.UserId,
                CharacterId = dto.CharacterId,
                Revision = (ulong)dto.Revision,
                UpdatedAt = Timestamp.FromDateTimeOffset(dto.UpdatedAt)
            };

            resp.Equips.AddRange(
                dto.Equips.Select(e => new UserCharacterEquipPb
                {
                    EquipId = e.equipId,
                    InventoryId = e.inventoryId ?? 0 // null → 0
                })
            );

            return resp;
        }

        public static SetEquipmentResponse ToPbSet(this SetEquipmentResultDto dto)
        {
            var resp = new SetEquipmentResponse
            {
                UserId = dto.UserId,
                CharacterId = dto.CharacterId,
                Revision = (ulong)dto.Revision,
                UpdatedAt = Timestamp.FromDateTimeOffset(dto.UpdatedAt),
                Slot = new EquipmentSlotStatePb
                {
                    EquipId = dto.Slot.equipId
                }
            };

            if (dto.Slot.inventoryId.HasValue)
            {
                resp.Slot.InventoryId = dto.Slot.inventoryId.Value; // optional 세팅
            }

            resp.Equips.AddRange(
                dto.Equips.Select(e => new UserCharacterEquipPb
                {
                    EquipId = e.equipId,
                    InventoryId = e.inventoryId ?? 0
                })
            );

            return resp;
        }
    }
}

