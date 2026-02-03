using Application.Common.Interface;
using Application.Repositories;
using Application.UserCurrency;
using Application.UserInventory;
using Microsoft.Extensions.Logging;
using static Application.Shop.ShopErrorCodes;

namespace Application.Shop
{
    public sealed class PurchaseService : IPurchaseService
    {
        private readonly IShopRepository _shopRepo;
        private readonly IShopProductRepository _productRepo;
        private readonly IShopQueryRepository _shopQueryRepo;
        private readonly IUserPurchaseLogRepository _logRepo;
        private readonly IUserPurchaseLogQueryRepository _logQueryRepo;
        private readonly IWalletService _wallet;
        private readonly IUserInventoryService _inventory;
        private readonly ICurrencyRepository _currencyRepo;
        private readonly IDistributedLock _lock;
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;
        private readonly ILogger<PurchaseService> _log;

        private static readonly TimeSpan LockExpiry = TimeSpan.FromSeconds(10);

        public PurchaseService(
            IShopRepository shopRepo,
            IShopProductRepository productRepo,
            IShopQueryRepository shopQueryRepo,
            IUserPurchaseLogRepository logRepo,
            IUserPurchaseLogQueryRepository logQueryRepo,
            IWalletService wallet,
            IUserInventoryService inventory,
            ICurrencyRepository currencyRepo,
            IDistributedLock @lock,
            IUnitOfWork uow,
            IClock clock,
            ILogger<PurchaseService> log)
        {
            _shopRepo = shopRepo;
            _productRepo = productRepo;
            _shopQueryRepo = shopQueryRepo;
            _logRepo = logRepo;
            _logQueryRepo = logQueryRepo;
            _wallet = wallet;
            _inventory = inventory;
            _currencyRepo = currencyRepo;
            _lock = @lock;
            _uow = uow;
            _clock = clock;
            _log = log;
        }

        // ───── 유저별 상점 목록 ─────

        public async Task<IReadOnlyList<UserShopDto>> GetShopListForUserAsync(
            int userId, CancellationToken ct)
        {
            var now = _clock.UtcNow;
            var shops = await _shopQueryRepo.GetActiveShopsAsync(now, ct);

            var todayUtc = now.Date;
            var weekStartUtc = todayUtc.AddDays(-(int)todayUtc.DayOfWeek + (int)DayOfWeek.Monday);
            if (todayUtc.DayOfWeek == DayOfWeek.Sunday)
                weekStartUtc = weekStartUtc.AddDays(-7);

            var result = new List<UserShopDto>();

            foreach (var shop in shops)
            {
                var productDtos = new List<UserShopProductDto>();

                foreach (var product in shop.Products)
                {
                    var counts = await _logQueryRepo.GetPurchaseCountsAsync(
                        userId, product.Id,
                        new DateTimeOffset(todayUtc, TimeSpan.Zero),
                        new DateTimeOffset(weekStartUtc, TimeSpan.Zero),
                        ct);

                    productDtos.Add(new UserShopProductDto(
                        product.ToDto(),
                        counts.Daily,
                        counts.Weekly,
                        counts.Total));
                }

                result.Add(new UserShopDto(shop.ToDto(), productDtos));
            }

            return result;
        }

        // ───── 구매 ─────

        public async Task<PurchaseResult> PurchaseAsync(
            int userId, int shopProductId, int quantity, CancellationToken ct)
        {
            var lockKey = $"shop:purchase:{userId}";

            if (!await _lock.AcquireAsync(lockKey, LockExpiry))
                return PurchaseResult.Fail(PurchaseInProgress);

            try
            {
                return await ExecutePurchaseAsync(userId, shopProductId, quantity, ct);
            }
            finally
            {
                await _lock.ReleaseAsync(lockKey);
            }
        }

        private async Task<PurchaseResult> ExecutePurchaseAsync(
            int userId, int shopProductId, int quantity, CancellationToken ct)
        {
            var now = _clock.UtcNow;

            // 1. 상품 조회
            var product = await _productRepo.GetByIdAsync(shopProductId, ct);
            if (product is null)
                return PurchaseResult.Fail(ProductNotFound);

            if (!product.IsActive)
                return PurchaseResult.Fail(ProductNotActive);

            // 2. 상점 검증
            var shop = await _shopRepo.GetByIdAsync(product.ShopId, ct);
            if (shop is null)
                return PurchaseResult.Fail(ShopNotFound);

            if (!shop.IsActive)
                return PurchaseResult.Fail(ShopNotActive);

            if (!shop.IsOpenAt(now))
                return PurchaseResult.Fail(ShopNotInPeriod);

            // 3. 구매 제한 검증
            var todayUtc = now.Date;
            var weekStartUtc = todayUtc.AddDays(-(int)todayUtc.DayOfWeek + (int)DayOfWeek.Monday);
            if (todayUtc.DayOfWeek == DayOfWeek.Sunday)
                weekStartUtc = weekStartUtc.AddDays(-7);

            var counts = await _logQueryRepo.GetPurchaseCountsAsync(
                userId, shopProductId,
                new DateTimeOffset(todayUtc, TimeSpan.Zero),
                new DateTimeOffset(weekStartUtc, TimeSpan.Zero),
                ct);

            if (product.DailyLimit.HasValue && counts.Daily + quantity > product.DailyLimit.Value)
                return PurchaseResult.Fail(DailyLimitExceeded);

            if (product.WeeklyLimit.HasValue && counts.Weekly + quantity > product.WeeklyLimit.Value)
                return PurchaseResult.Fail(WeeklyLimitExceeded);

            if (product.TotalLimit.HasValue && counts.Total + quantity > product.TotalLimit.Value)
                return PurchaseResult.Fail(TotalLimitExceeded);

            // 4. 재화 코드 조회
            var currency = await _currencyRepo.GetByIdAsync((short)product.CurrencyId, ct)
                ?? throw new InvalidOperationException($"Currency {product.CurrencyId} not found.");

            var totalPrice = product.Price * quantity;
            var currencyCode = currency.Code;

            // 5. 트랜잭션: 재화 차감 → 아이템 지급 → 기록 저장
            var result = await _uow.ExecuteInTransactionAsync(async () =>
            {
                // 재화 차감
                var spent = await _wallet.SpendAsync(userId, currencyCode, totalPrice, ct);
                if (!spent)
                    return PurchaseResult.Fail(InsufficientCurrency);

                // 아이템 지급
                var totalItems = product.QuantityPerPurchase * quantity;
                var inventoryResult = await _inventory.GrantAsync(
                    new GrantItemRequest(userId, product.ItemId, totalItems), ct);

                // 구매 기록
                var log = Domain.Entities.Shop.UserPurchaseLog.Create(
                    userId, shopProductId, quantity, totalPrice, currencyCode, now);
                await _logRepo.AddAsync(log, ct);

                // 잔액 조회
                var balances = await _wallet.GetBalancesAsync(userId, ct);
                var remaining = balances.FirstOrDefault(b => b.Code == currencyCode).Amount;

                return PurchaseResult.Ok(
                    remaining,
                    inventoryResult.Count,
                    counts.Daily + quantity,
                    counts.Weekly + quantity,
                    counts.Total + quantity);
            }, ct);

            _log.LogInformation(
                "Purchase {Result} user={UserId} product={ProductId} qty={Qty}",
                result.Success ? "OK" : result.ErrorCode, userId, shopProductId, quantity);

            return result;
        }
    }
}
