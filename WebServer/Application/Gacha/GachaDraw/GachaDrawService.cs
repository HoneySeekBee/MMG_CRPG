using Application.Gacha.GachaBanner;
using Application.Gacha.GachaPool;
using Application.Repositories;
using System.Text.Json;
using Application.UserCurrency;
using Application.Common;
using Application.Users.Caching;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Gacha.GachaDraw
{
    public record PayCostResult(
        int UsedTickets,
        long UsedCurrency
    );
    public sealed class GachaDrawService : IGachaDrawService
    {
        private readonly IGachaBannerService _banners;
        private readonly IGachaPoolService _pools;

        private readonly IGachaDrawLogRepository _logRepo;
        private readonly IUserCharacterRepository _charRepo;
        private readonly IItemRepository _itemRepo;
        private readonly IUserInventoryRepository _inventoryRepo;
        private readonly IWalletService _walletService;
        private readonly ICurrencyRepository _currencyRepo;
        private readonly IGachaCacheService _cache;
        private readonly IUserCacheService _userCache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Random _rng = new();

        private readonly ILogger<GachaDrawService> _logger;

        public GachaDrawService(
            IGachaBannerService banners,
            IGachaPoolService pools,
            IGachaDrawLogRepository logRepo,
            IUserCharacterRepository charRepo,
            IItemRepository itemRepo,
            IUserInventoryRepository inventoryRepo,
            IWalletService wallet,
            ICurrencyRepository currencyRepo,
            IGachaCacheService cache,
            IUserCacheService userCache,
            IUnitOfWork unitOfWork,
            ILogger<GachaDrawService> logger)
        {
            _banners = banners;
            _pools = pools;
            _logRepo = logRepo;
            _charRepo = charRepo;
            _itemRepo = itemRepo;
            _inventoryRepo = inventoryRepo;
            _walletService = wallet;
            _currencyRepo = currencyRepo;
            _cache = cache;
            _userCache = userCache;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<DrawResultDto> DrawAsync(string bannerKey, int count, int userId, CancellationToken ct)
        {
            // 1) Redis → 없으면 DB (트랜잭션 밖에서 조회)
            var banner = await GetBannerAsync(bannerKey, ct);
            var pool = await GetPoolAsync(banner.GachaPoolId, ct);

            // 2) 활성 상태 체크
            var now = DateTimeOffset.UtcNow;
            if (!banner.IsActive || banner.StartsAt > now || (banner.EndsAt != null && banner.EndsAt < now))
                throw new InvalidOperationException("Banner is not active");

            if (pool.Entries.Count == 0)
                throw new InvalidOperationException("Pool entries missing");

            _logger.LogInformation(
                "[GachaDraw] start userId={UserId} bannerKey={BannerKey} count={Count} trace={TraceId}",
                userId, bannerKey, count,
                Activity.Current?.TraceId.ToString() ?? "-");

            // 3) 결과 미리 생성 (트랜잭션 밖 - 순수 계산)
            var pulledEntries = new List<GachaPoolEntryDto>(count);
            bool hasGuaranteed = false;

            for (int i = 0; i < count; i++)
            {
                var entry = Pick(pool.Entries);
                pulledEntries.Add(entry);

                if (entry.Grade >= 4)
                    hasGuaranteed = true;
            }

            bool guaranteeApplied = false;
            var highGrades = pool.Entries.Where(e => e.Grade >= 4).ToList();

            if (count >= 10 && !hasGuaranteed && highGrades.Count > 0)
            {
                pulledEntries[count - 1] = highGrades
                    .OrderByDescending(e => e.Grade)
                    .First();
                guaranteeApplied = true;
            }

            // 4) 트랜잭션 내에서 비용 차감 + 보상 지급 + 로그 저장
            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // 4-1) 비용 차감
                    var pay = await PayCostAsync(userId, banner, count, ct);

                    // 4-2) 배치 처리로 N+1 해결
                    var results = await GrantRewardsBatchAsync(userId, pulledEntries, guaranteeApplied, ct);

                    // 4-3) 로그 저장
                    var log = new Domain.Entities.Gacha.GachaDrawLog
                    {
                        UserId = userId,
                        BannerId = banner.Id,
                        PoolId = pool.Pool.PoolId,
                        Timestamp = now,
                        ResultsJson = JsonSerializer.Serialize(results)
                    };
                    await _logRepo.AddAsync(log, ct);

                    _logger.LogInformation(
                        "[GachaDraw] success userId={UserId} usedTickets={Tickets} usedCurrency={Currency}",
                        userId, pay.UsedTickets, pay.UsedCurrency);

                    return new DrawResultDto(now, results, pay.UsedTickets, pay.UsedCurrency);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[GachaDraw] FAILED userId={UserId} bannerKey={BannerKey} count={Count} error={Error}",
                    userId, bannerKey, count, ex.Message);
                throw;
            }
        }
        private async Task<GachaBannerDto> GetBannerAsync(string key, CancellationToken ct)
        {
            // Redis 조회
            var cached = await _cache.GetActiveBannersAsync(ct);
            var banner = cached.FirstOrDefault(b => b.Key == key);

            if (banner != null)
                return banner;

            // fallback DB
            banner = await _banners.GetByKeyAsync(key, ct)
                     ?? throw new GameErrorException("GACHA_BANNER_NOT_FOUND", "Banner not found");

            return banner;
        }
        private async Task<GachaPoolDetailDto> GetPoolAsync(int poolId, CancellationToken ct)
        {
            // Redis 조회
            var pool = await _cache.GetPoolAsync(poolId, ct);
            if (pool != null)
                return pool;

            // fallback DB
            pool = await _pools.GetDetailAsync(poolId, ct)
                   ?? throw new GameErrorException("GACHA_POOL_NOT_FOUND", "Pool not found");

            return pool;
        }
        /// <summary>
        /// 배치 처리로 N+1 문제 해결
        /// - 캐릭터 보유 여부 한 번에 조회
        /// - 파편 아이템 한 번에 조회
        /// - 신규 캐릭터 한 번에 추가
        /// </summary>
        private async Task<List<DrawResultItemDto>> GrantRewardsBatchAsync(
            int userId,
            List<GachaPoolEntryDto> pulledEntries,
            bool guaranteeApplied,
            CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;
            var results = new List<DrawResultItemDto>(pulledEntries.Count);

            // 1) 뽑은 캐릭터 ID 목록 (중복 제거)
            var pulledCharacterIds = pulledEntries.Select(e => e.CharacterId).Distinct().ToList();

            // 2) 유저가 이미 보유한 캐릭터 ID 한 번에 조회
            var ownedIds = await _charRepo.GetOwnedCharacterIdsAsync(userId, pulledCharacterIds, ct);

            // 3) 파편 코드 목록 생성 및 한 번에 조회
            var shardCodes = pulledCharacterIds.Select(id => $"SHARD_{id}").ToList();
            var shardItems = await _itemRepo.GetByCodesAsync(shardCodes, ct);

            // 4) 이번 요청에서 신규로 지급한 캐릭터 추적
            var newlyGrantedThisRequest = new HashSet<int>();
            var newCharactersToAdd = new List<Domain.Entities.User.UserCharacter>();

            // 5) 결과 처리
            for (int i = 0; i < pulledEntries.Count; i++)
            {
                var entry = pulledEntries[i];
                bool isGuaranteed = guaranteeApplied && i == pulledEntries.Count - 1;
                bool isNew = false;
                bool isShard = false;
                int shardAmount = 0;

                // 이미 보유 중이거나 이번 요청에서 이미 지급한 경우 → 파편
                if (ownedIds.Contains(entry.CharacterId) || newlyGrantedThisRequest.Contains(entry.CharacterId))
                {
                    isShard = true;
                    shardAmount = GetShardAmount(entry.Grade);

                    var shardCode = $"SHARD_{entry.CharacterId}";
                    if (!shardItems.TryGetValue(shardCode, out var shard))
                        throw new InvalidOperationException($"Shard not found: {shardCode}");

                    await _inventoryRepo.AddItemAsync(userId, shard.Id, shardAmount, ct);
                }
                else
                {
                    // 신규 캐릭터
                    isNew = true;
                    var newChar = Domain.Entities.User.UserCharacter.Create(userId, entry.CharacterId, now);
                    newCharactersToAdd.Add(newChar);
                    newlyGrantedThisRequest.Add(entry.CharacterId);
                }

                results.Add(new DrawResultItemDto(
                    entry.CharacterId, entry.Grade, entry.RateUp,
                    isNew, isShard, shardAmount, isGuaranteed));
            }

            // 6) 신규 캐릭터 한 번에 추가
            if (newCharactersToAdd.Count > 0)
            {
                await _charRepo.AddRangeAsync(newCharactersToAdd, ct);
            }

            return results;
        }
        // Character 지급
        private async Task<DrawResultItemDto> GrantCharacterOrShardAsync(int userId, GachaPoolEntryDto entry, CancellationToken ct)
        {
            var existing = await _charRepo.GetAsync(userId, entry.CharacterId, ct);
            var now = DateTimeOffset.UtcNow;

            // 1) 중복 → 파편 지급
            if (existing != null)
            {
                string shardCode = $"SHARD_{entry.CharacterId}";
                var shard = await _itemRepo.GetByCodeAsync(shardCode, false, ct)
                    ?? throw new InvalidOperationException($"Shard not found: {shardCode}");

                int amount = entry.Grade switch
                {
                    3 => 5,
                    4 => 10,
                    5 => 15,
                    6 => 20,
                    7 => 30,
                    _ => 5
                };

                await _inventoryRepo.AddItemAsync(userId, shard.Id, amount, ct);

                return new DrawResultItemDto(
                    entry.CharacterId,
                    entry.Grade,
                    entry.RateUp,
                    IsNew: false,
                    IsShard: true,
                    ShardAmount: amount,
                    IsGuaranteed: false
                );
            }

            // 2) 신규 캐릭터
            var newChar = Domain.Entities.User.UserCharacter.Create(userId, entry.CharacterId, now);
            await _charRepo.AddAsync(newChar, ct);

            return new DrawResultItemDto(
                entry.CharacterId,
                entry.Grade,
                entry.RateUp,
                IsNew: true,
                IsShard: false,
                ShardAmount: 0,
                IsGuaranteed: false
            );
        }
        private int GetShardAmount(int grade)
        {
            return grade switch
            {
                3 => 5,
                4 => 10,
                5 => 15,
                6 => 20,
                7 => 30,
                _ => 5
            };
        }
        private GachaPoolEntryDto PickRandomEntry(IEnumerable<GachaPoolEntryDto> entries)
        {
            int total = entries.Sum(e => e.Weight);
            int roll = _rng.Next(1, total + 1);

            int acc = 0;
            foreach (var e in entries)
            {
                acc += e.Weight;
                if (roll <= acc)
                    return e;
            }
            return entries.Last();
        }
        public async Task<PayCostResult> PayCostAsync(int userId, GachaBannerDto banner, int count, CancellationToken ct)
        {
            int needTickets = count;
            int perCost = banner.Cost;

            var inv = await _inventoryRepo.GetByKeyAsync(userId, banner.TicketItemId, ct);
            int ownedTickets = inv?.Count ?? 0;

            int useTickets = Math.Min(ownedTickets, needTickets);
            int remaining = needTickets - useTickets;

            long gemCost = (long)remaining * perCost;

            // Currency 정보
            var currency = await _currencyRepo.GetByIdAsync((short)banner.CostCurrencyId, ct)
                ?? throw new GameErrorException("GACHA_INVALID_CURRENCY", "Currency not found");

            // 1) GEM 차감
            if (gemCost > 0)
            {
                bool ok = await _walletService.SpendAsync(userId, currency.Code, gemCost, ct);
                if (!ok)
                    throw new GameErrorException("GACHA_NOT_ENOUGH_COST", "Not enough currency");

                await _userCache.InvalidateWalletAsync(userId, ct);
            }

            // 2) 티켓 차감 (SaveChanges는 트랜잭션 끝에서 처리)
            if (useTickets > 0 && inv != null)
            {
                inv.TryConsume(useTickets);
            }

            return new PayCostResult(useTickets, gemCost);
        }
        private GachaPoolEntryDto Pick(IReadOnlyList<GachaPoolEntryDto> entries)
        {
            int total = entries.Sum(e => e.Weight);
            int roll = _rng.Next(1, total + 1);

            int acc = 0;
            foreach (var e in entries)
            {
                acc += e.Weight;
                if (roll <= acc)
                    return e;
            }
            return entries.Last(); // fallback
        }
    }
}
