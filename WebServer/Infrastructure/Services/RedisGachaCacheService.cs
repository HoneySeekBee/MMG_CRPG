using Application.Gacha;
using Application.Gacha.GachaBanner;
using Application.Gacha.GachaPool;
using Application.Repositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public sealed class RedisGachaCacheService : IGachaCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IGachaBannerRepository _banners; 
        private readonly IGachaPoolService _pools;

        private readonly IDatabase _db;

        public RedisGachaCacheService(
      IConnectionMultiplexer redis,
      IGachaBannerRepository banners,
      IGachaPoolService pools)
        {
            _redis = redis;
            _db = redis.GetDatabase();
            _banners = banners;
            _pools = pools;
        }

        private const string ActiveBannerKey = "gacha:banners:active";
        private static string PoolKey(int id) => $"gacha:pool:{id}";
         
        // 1) Banner 조회 
        public async Task<IReadOnlyList<GachaBannerDto>> GetActiveBannersAsync(CancellationToken ct)
        {
            var val = await _db.StringGetAsync(ActiveBannerKey);
            if (val.IsNullOrEmpty)
                return Array.Empty<GachaBannerDto>();

            return JsonSerializer.Deserialize<List<GachaBannerDto>>(val!)!;
        }
         
        // 2) Pool 조회 
        public async Task<GachaPoolDetailDto?> GetPoolAsync(int poolId, CancellationToken ct)
        {
            var val = await _db.StringGetAsync(PoolKey(poolId));
            if (val.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<GachaPoolDetailDto>(val!)!;
        }
         
        // 3) RefreshAll - Warmup 용 
        public async Task RefreshAllAsync(CancellationToken ct)
        {
            // 1) 활성 배너를 DB에서 읽기
            var active = await _banners.ListLiveAsync(
      now: null,
      take: 100,
      ct: ct
  );
            var bannerDtos = active.Select(b => b.ToDto()).ToList();
            await _db.StringSetAsync(ActiveBannerKey, JsonSerializer.Serialize(bannerDtos));

            // 2) 관련 Pool들을 Redis에 저장
            var poolIds = active.Select(a => a.GachaPoolId).Distinct().ToList();

            foreach (var pid in poolIds)
            {
                var poolDetail = await _pools.GetDetailAsync(pid, ct);
                if (poolDetail != null)
                {
                    await _db.StringSetAsync(PoolKey(pid),
                        JsonSerializer.Serialize(poolDetail));
                }
            }
        }
    }
} 
