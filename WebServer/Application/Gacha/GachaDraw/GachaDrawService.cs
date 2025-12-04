using Application.Gacha.GachaBanner;
using Application.Gacha.GachaPool;
using Application.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities.User;
using Application.UserCurrency;
using Application.Common;


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

        private readonly Random _rng = new();

        public GachaDrawService(
            IGachaBannerService banners,
            IGachaPoolService pools,
            IGachaDrawLogRepository logRepo,
            IUserCharacterRepository charRepo,
            IItemRepository itemRepo,
            IUserInventoryRepository inventoryRepo,
            IWalletService wallet,
            ICurrencyRepository currencyRepo,
            IGachaCacheService cache)
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
        }
        public async Task<DrawResultDto> DrawAsync(string bannerKey, int count, int userId, CancellationToken ct)
        {
            // 1) Redis → 없으면 DB
            var banner = await GetBannerAsync(bannerKey, ct);
            var pool = await GetPoolAsync(banner.GachaPoolId, ct);

            // 2) 활성 상태 체크
            var now = DateTimeOffset.UtcNow;
            if (!banner.IsActive || banner.StartsAt > now || (banner.EndsAt != null && banner.EndsAt < now))
                throw new InvalidOperationException("Banner is not active");

            if (pool.Entries.Count == 0)
                throw new InvalidOperationException("Pool entries missing");

            // 3) 비용 차감
            var pay = await PayCostAsync(userId, banner, count, ct);

            // 4) 결과 생성
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
            // 5) 결과 아이템 변환
            var results = new List<DrawResultItemDto>(count);

            for (int i = 0; i < pulledEntries.Count; i++)
            {
                var e = pulledEntries[i];

                var item = await GrantCharacterOrShardAsync(userId, e, ct);
                bool isGuaranteed = guaranteeApplied && i == pulledEntries.Count - 1;

                results.Add(item with { IsGuaranteed = isGuaranteed });
            }

            // 6) 로그 저장
            var log = new Domain.Entities.Gacha.GachaDrawLog
            {
                UserId = userId,
                BannerId = banner.Id,
                PoolId = pool.Pool.PoolId,
                Timestamp = now,
                ResultsJson = JsonSerializer.Serialize(results)
            };

            await _logRepo.AddAsync(log, ct);
            await _logRepo.SaveChangesAsync(ct);

            return new DrawResultDto(now, results, pay.UsedTickets, pay.UsedCurrency);
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
            }

            // 2) 티켓 차감
            if (useTickets > 0 && inv != null)
            {
                inv.TryConsume(useTickets);
                await _inventoryRepo.SaveChangesAsync(ct);
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
