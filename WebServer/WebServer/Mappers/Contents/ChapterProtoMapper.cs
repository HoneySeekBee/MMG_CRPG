using Application.Contents.Chapters;
using Contracts.Protos;
using Google.Protobuf.WellKnownTypes;

namespace WebServer.Mappers.Contents
{
    public static class ChapterProtoMapper
    {
        public static ChapterPb ToProto(ChapterDto dto)
        {
            return new ChapterPb
            {
                ChapterId = dto.ChapterId,
                BattleId = dto.BattleId,
                ChapterNum = dto.ChapterNum,
                Name = dto.Name ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                IsActive = dto.IsActive,
                CreatedAt = Timestamp.FromDateTime(dto.CreatedAt.ToUniversalTime()),
                UpdatedAt = Timestamp.FromDateTime(dto.UpdatedAt.ToUniversalTime())
            };
        }

        public static ChaptersPb ToProto(IEnumerable<ChapterDto> list)
        {
            var proto = new ChaptersPb();
            foreach (var item in list)
            {
                proto.Chapters.Add(ToProto(item));
            }
            return proto;
        }
    }
}
