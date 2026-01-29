using Application.Common.Interface;
using Application.Users.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RedisUserCacheService : IUserCacheService
    {
        private readonly ICacheService _cache; 

        public RedisUserCacheService(ICacheService cache)
        {
            _cache = cache;
        }
        // ---- Keys ----
        private static string ProfileCoreKey(int userId) => $"user:{userId}:profile_core";
        private static string WalletKey(int userId) => $"user:{userId}:wallet";

        // ---- TTL 정책 ----
        // ProfileCore: 20~30분 + 지터
        private static readonly TimeSpan ProfileCoreTtlBase = TimeSpan.FromMinutes(20);
        private static readonly TimeSpan ProfileCoreTtlJitter = TimeSpan.FromMinutes(10);

        // Wallet: 30~60초 + 지터
        private static readonly TimeSpan WalletTtlBase = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan WalletTtlJitter = TimeSpan.FromSeconds(30);

        private static TimeSpan WithJitter(TimeSpan baseTtl, TimeSpan jitter)
        { 
            var extraMs = Random.Shared.Next(0, (int)jitter.TotalMilliseconds + 1);
            return baseTtl + TimeSpan.FromMilliseconds(extraMs);
        }
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public async Task<UserProfileCoreCacheDto?> GetProfileCoreAsync(int userId, CancellationToken ct)
        {
            var key = ProfileCoreKey(userId);
            var raw = await _cache.GetAsync(key);
            if (raw is null) return null;

            try
            {
                return JsonSerializer.Deserialize<UserProfileCoreCacheDto>(raw, JsonOpts);
            }
            catch
            {
                // 캐시 깨짐/버전 불일치 -> 삭제 후 미스로 처리
                await _cache.RemoveAsync(key);
                return null;
            }
        }
        public Task SetProfileCoreAsync(int userId, UserProfileCoreCacheDto dto, CancellationToken ct)
        {
            var key = ProfileCoreKey(userId);
            var ttl = WithJitter(ProfileCoreTtlBase, ProfileCoreTtlJitter);
            var json = JsonSerializer.Serialize(dto, JsonOpts);
            return _cache.SetAsync(key, json, ttl);
        } 
        public Task InvalidateProfileCoreAsync(int userId, CancellationToken ct)
        {
            return _cache.RemoveAsync(ProfileCoreKey(userId));
        }

        public async Task<UserWalletCacheDto?> GetWalletAsync(int userId, CancellationToken ct)
        {
            var key = WalletKey(userId);
            var raw = await _cache.GetAsync(key);
            if (raw is null) return null;

            try
            {
                return JsonSerializer.Deserialize<UserWalletCacheDto>(raw, JsonOpts);
            }
            catch
            {
                await _cache.RemoveAsync(key);
                return null;
            }
        } 
        public Task SetWalletAsync(int userId, UserWalletCacheDto dto, CancellationToken ct)
        {
            var key = WalletKey(userId);
            var ttl = WithJitter(WalletTtlBase, WalletTtlJitter);
            var json = JsonSerializer.Serialize(dto, JsonOpts);
            return _cache.SetAsync(key, json, ttl);
        }

        public Task InvalidateWalletAsync(int userId, CancellationToken ct)
        {
            return _cache.RemoveAsync(WalletKey(userId));
        }

        public async Task InvalidateUserAsync(int userId, CancellationToken ct)
        {
            await _cache.RemoveAsync(ProfileCoreKey(userId));
            await _cache.RemoveAsync(WalletKey(userId));
        }

    }
}
