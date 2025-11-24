using Application.Common.Interface;
using Domain.Entities;
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

        public RedisSessionStorage(IConnectionMultiplexer connection)
        {
            _redis = connection.GetDatabase();
        }

        public async Task StoreSessionAsync(Session session, CancellationToken ct)
        {
            string key = $"session:refresh:{session.RefreshTokenHash}";

            var data = JsonSerializer.Serialize(session);

            var ttl = session.RefreshExpiresAt - DateTimeOffset.UtcNow;
            await _redis.StringSetAsync(key, data, ttl);
        }

        public async Task<Session?> GetByRefreshHashAsync(string refreshHash, CancellationToken ct)
        {
            string key = $"session:refresh:{refreshHash}";
            var value = await _redis.StringGetAsync(key);

            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<Session>(value!)!;
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
