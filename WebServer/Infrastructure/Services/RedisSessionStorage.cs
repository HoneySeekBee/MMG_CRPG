using Amazon.Runtime.Internal.Util;
using Application.Common.Interface;
using Application.Gacha.GachaDraw;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RedisSessionStorage : ISessionStorage
    {
        private readonly IDatabase _redis;

        private readonly ILogger<GachaDrawService> _logger;

        public RedisSessionStorage(IConnectionMultiplexer connection,
             ILogger<GachaDrawService> logger)
        {
            _redis = connection.GetDatabase();
            _logger = logger;
        }

        public async Task StoreSessionAsync(Session session, CancellationToken ct)
        {
            string key = $"session:refresh:{session.RefreshTokenHash}";
            var dto = RedisSessionMapper.ToDto(session);

            var data = JsonSerializer.Serialize(dto);

            var ttl = session.RefreshExpiresAt - DateTimeOffset.UtcNow;
            if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromSeconds(1);
            
            await _redis.StringSetAsync(key, data, ttl);
        }

        public async Task<Session?> GetByRefreshHashAsync(string refreshHash, CancellationToken ct)
        {
            string key = $"session:refresh:{refreshHash}";
            var value = await _redis.StringGetAsync(key);

            if (!value.HasValue)
                return null;

            try
            {
                var dto = JsonSerializer.Deserialize<RedisSessionDto>(value!);
                return dto == null ? null : RedisSessionMapper.ToDomain(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SESSION_DESERIALIZE_FAIL key={Key}", key);
                await _redis.KeyDeleteAsync(key);
                return null;
            }
        }

        public async Task RevokeAsync(string refreshHash, CancellationToken ct)
        {
            string key = $"session:refresh:{refreshHash}";
            await _redis.KeyDeleteAsync(key);
        }
        public async Task RevokeAllByUserIdAsync(int userId)
        {
            var endpoints = _redis.Multiplexer.GetEndPoints();
            foreach (var ep in endpoints)
            {
                var server = _redis.Multiplexer.GetServer(ep);
                var keys = server.Keys(pattern: $"session:*");

                foreach (var key in keys)
                {
                    var json = await _redis.StringGetAsync(key);
                    if (!json.HasValue) continue;

                    var session = JsonSerializer.Deserialize<Session>(json!);
                    if (session!.UserId == userId)
                    {
                        await _redis.KeyDeleteAsync(key);
                    }
                }
            }
        }
    }
}
