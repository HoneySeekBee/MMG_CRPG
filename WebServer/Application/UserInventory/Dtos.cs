using UserInven = Domain.Entities.UserInventory;

namespace Application.UserInventory
{
    public sealed record UserInventoryDto(
        int UserId,
        int ItemId,
        int Count,
        DateTimeOffset UpdatedAt
    );
    public sealed record UserInventoryListDto(
        int UserId,
        IReadOnlyList<UserInventoryDto> Items
    );
    public static class UserInventoryMapping
    {
        public static UserInventoryDto ToDto(this UserInven entity)
            => new(entity.UserId, entity.ItemId, entity.Count, entity.UpdatedAt);

        public static IReadOnlyList<UserInventoryDto> ToDtoList(this IEnumerable<UserInven> entities)
            => entities.Select(e => e.ToDto()).ToList();
    }
}
