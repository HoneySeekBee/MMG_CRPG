using Application.UserCharacter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserCharacterEquips
{
    public sealed record SetEquipmentCommand(int UserId, int CharacterId, int EquipId, long? InventoryId);

    public sealed record UserCharacterEquipSnapshotDto(
    int UserId,
    int CharacterId,
    int Revision,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<UserCharacterEquipDto> Equips // 네가 이미 가진 DTO record
);
    public sealed record SetEquipmentResultDto(
    int UserId,
    int CharacterId,
    int Revision,
    DateTimeOffset UpdatedAt,
    UserCharacterEquipDto Slot,                      // 방금 바뀐 슬롯 상태(InventoryId null일 수 있음)
    IReadOnlyList<UserCharacterEquipDto> Equips      // 최종 스냅샷
);
}
