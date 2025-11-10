using Application.UserCharacter;
using Contracts.Protos;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;

namespace WebServer.Mappers
{
    internal static class UserCharacterPbMappings
    {
        // IReadOnlyList 오버로드: IEnumerable 버전으로 위임
        public static UserCharacterListPb ToPb(this IReadOnlyList<UserCharacterDto> list)
            => ((IEnumerable<UserCharacterDto>)list).ToPb();

        // IEnumerable 버전(기존)
        public static UserCharacterListPb ToPb(this IEnumerable<UserCharacterDto> list)
        {
            var result = new UserCharacterListPb();
            foreach (var dto in list)
            {
                var summary = new UserCharacterSummaryPb
                {
                    UserId = dto.UserId,
                    CharacterId = dto.CharacterId,
                    Level = dto.Level,
                    Exp = dto.Exp,
                    BreakThrough = dto.BreakThrough,
                    UpdatedAt = Timestamp.FromDateTimeOffset(dto.UpdatedAt),
                    UserCharacterId = dto.UserCharacterId
                };

                if (dto.Skills is not null)
                {
                    summary.Skills.AddRange(dto.Skills.Select(s => new UserCharacterSkillPb
                    {
                        SkillId = s.SkillId,
                        Level = (uint)s.Level,
                        UpdatedAt = Timestamp.FromDateTimeOffset(s.UpdatedAt)
                    }));
                }

                if(dto.equips is not null)
                {
                    summary.Equips.AddRange(dto.equips.Select(s => new UserCharacterEquipPb
                    {
                        EquipId = s.equipId,
                        InventoryId = s.inventoryId?? 0,
                    }));
                }

                result.Characters.Add(summary);
            }
            return result;
        }
    }
}
