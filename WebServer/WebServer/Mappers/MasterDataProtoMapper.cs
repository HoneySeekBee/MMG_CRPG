using Application.Elements;
using Application.Factions;
using Application.Rarities;
using Application.Roles;
using Game.MasterData;

namespace WebServer.Mappers
{
    public static class MasterDataProtoMapper
    {
        public static RarityMessage ToProto(this RarityDto dto) => new RarityMessage
        {
            RarityId = dto.RarityId,
            Stars = dto.Stars,
            Key = dto.Key,
            Label = dto.Label,
            ColorHex = dto.ColorHex ?? "",
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            Meta = dto.Meta ?? ""
        };

        public static ElementMessage ToProto(this ElementDto dto) => new ElementMessage
        {
            ElementId = dto.ElementId,
            Key = dto.Key,
            Label = dto.Label,
            IconId = dto.IconId ?? 0,
            ColorHex = dto.ColorHex,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            Meta = dto.Meta ?? "",
            CreatedAt = dto.CreatedAt.ToUnixTimeMilliseconds().ToString(),
            UpdatedAt = dto.UpdatedAt.ToUnixTimeMilliseconds().ToString(),
        };

        public static RoleMessage ToProto(this RoleDto dto) => new RoleMessage
        {
            RoleId = dto.RoleId,
            Key = dto.Key,
            Label = dto.Label,
            IconId = dto.IconId ?? 0,
            ColorHex = dto.ColorHex ?? "",
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            Meta = dto.Meta ?? ""
        };

        public static FactionMessage ToProto(this FactionDto dto) => new FactionMessage
        {
            FactionId = dto.FactionId,
            Key = dto.Key,
            Label = dto.Label,
            IconId = dto.IconId ?? 0,
            ColorHex = dto.ColorHex ?? "",
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            Meta = dto.Meta ?? ""
        };
    }

}
