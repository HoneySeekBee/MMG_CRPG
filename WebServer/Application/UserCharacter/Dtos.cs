using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserCha = Domain.Entities.User.UserCharacter;

namespace Application.UserCharacter
{
    public sealed record UserCharacterDto(
    int UserId, int CharacterId, int Level, int Exp, int BreakThrough, DateTimeOffset UpdatedAt,
    IReadOnlyList<UserCharacterSkillDto> Skills,
    IReadOnlyList<UserCharacterEquipDto> equips);

    public sealed record UserCharacterSkillDto(int SkillId, int Level, DateTimeOffset UpdatedAt);

    public sealed record UserCharacterEquipDto(int UserId, int Characterid, short slotId, int? ItemId);

    public static class UserCharacterMappings
    {
        public static UserCharacterDto ToDto(this UserCha e) =>
            new(
                e.UserId, e.CharacterId, e.Level, e.Exp, e.BreakThrough, e.UpdatedAt,
                e.Skills.Select(s => new UserCharacterSkillDto(s.SkillId, s.Level, s.UpdatedAt)).ToList(), 
                e.Equips.Select(s => new UserCharacterEquipDto(s.UserId, s.CharacterId, s.SlotId, s.ItemId)).ToList()
            );
    }

}
