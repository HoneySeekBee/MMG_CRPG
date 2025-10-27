using Application.Repositories;
using Application.UserCharacter;
using Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserCharacterEquips
{
    public class CharacterEquipmentService : ICharacterEquipmentService
    {
        private readonly IUserCharacterEquipRepository _equips;

        public CharacterEquipmentService(IUserCharacterEquipRepository equips)
        {
            _equips = equips;
        }

        public async Task<UserCharacterEquipSnapshotDto> GetAsync(int userId, int characterId, CancellationToken ct)
        {
            var rows = await _equips.ListByCharacterAsync(userId, characterId, ct);

            // Revision, UpdatedAt은 도메인에 없다면 임시값 사용
            int revision = 0;
            DateTimeOffset updatedAt = DateTimeOffset.UtcNow;

            var dtoList = rows
                .Select(x => new UserCharacterEquipDto(x.UserId, x.CharacterId, x.EquipId, x.InventoryId))
                .ToList()
                .AsReadOnly();

            return new UserCharacterEquipSnapshotDto(
                userId,
                characterId,
                revision,
                updatedAt,
                dtoList
            );
        }

        public async Task<SetEquipmentResultDto> SetAsync(SetEquipmentCommand cmd, CancellationToken ct)
        {
            var row = await _equips.GetAsync(cmd.UserId, cmd.CharacterId, cmd.EquipId, ct);

            if (cmd.InventoryId is long invId)
            {
                // 장착
                if (row == null)
                {
                    row = UserCharacterEquip.Create(cmd.UserId, cmd.CharacterId, cmd.EquipId, invId);
                    await _equips.AddAsync(row, ct);
                }
                else
                {
                    row.Equip(invId);
                    await _equips.UpdateAsync(row, ct);
                }
            }
            else
            {
                // 해제
                if (row != null)
                {
                    row.Unequip();
                    await _equips.UpdateAsync(row, ct);
                }
                // row가 없으면 아무것도 하지 않음 (멱등)
            }

            // 저장
            await _equips.SaveChangesAsync(ct);

            // 스냅샷 재구성
            var all = await _equips.ListByCharacterAsync(cmd.UserId, cmd.CharacterId, ct);

            var dtoList = all
                .Select(x => new UserCharacterEquipDto(x.UserId, x.CharacterId, x.EquipId, x.InventoryId))
                .ToList()
                .AsReadOnly();

            var slotDto = new UserCharacterEquipDto(
                cmd.UserId,
                cmd.CharacterId,
                cmd.EquipId,
                row?.InventoryId
            );

            // Revision/UpdatedAt은 임시
            return new SetEquipmentResultDto(
                cmd.UserId,
                cmd.CharacterId,
                0,
                DateTimeOffset.UtcNow,
                slotDto,
                dtoList
            );
        } 
    }
}
