using Domain.Entities.Shop;

namespace Application.Repositories
{
    public sealed record PurchaseCounts(int Daily, int Weekly, int Total)
    {
        public static readonly PurchaseCounts Zero = new(0, 0, 0);
    }

    public interface IUserPurchaseLogQueryRepository
    {
        // 한방 쿼리로 일일/주간/총 구매 횟수 조회 (구매 제한 검증용)
        Task<PurchaseCounts> GetPurchaseCountsAsync(
            int userId,
            int productId,
            DateTimeOffset todayUtcStart,
            DateTimeOffset weekStartUtc,
            CancellationToken ct);

        // 구매 기록 목록 (운영툴 로그 페이지)
        Task<(IReadOnlyList<UserPurchaseLog> Items, int TotalCount)> GetLogsPagedAsync(
            int? userId,
            int? shopProductId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int page,
            int pageSize,
            CancellationToken ct);
    }
}
