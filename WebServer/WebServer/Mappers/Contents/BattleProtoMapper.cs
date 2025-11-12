using Application.Contents.Battles;
using Contracts.Protos;
using Domain.Entities.Contents;
using Google.Protobuf.WellKnownTypes;

namespace WebServer.Mappers.Contents
{
    public static class BattleProtoMapper
    {
        public static BattlePb ToProto(BattleDto dto)
        {
            return new BattlePb
            {
                Id = dto.Id,
                Name = dto.Name ?? string.Empty,
                Active = dto.Active,
                SceneKey = dto.SceneKey ?? string.Empty,
                CheckMulti = dto.CheckMulti,
                CreatedAt = Timestamp.FromDateTime(dto.CreatedAt.ToUniversalTime()),
                UpdatedAt = Timestamp.FromDateTime(dto.UpdatedAt.ToUniversalTime())
            };
        }

        public static BattlesPb ToProto(IEnumerable<BattleDto> list)
        {
            var result = new BattlesPb();
            foreach (var dto in list)
            {
                result.Battles.Add(ToProto(dto));
            }
            return result;
        }
    }
}
