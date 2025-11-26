using Application.Common.Interface;
using Domain.Common;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class RedisServerStatusTracker : IServerStatusTracker
    {
        private readonly IDatabase _db;

        public RedisServerStatusTracker(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task UpdateHeartbeatAsync(string serverId, ServerStatus status, CancellationToken ct = default)
        {
            var key = $"server:status:{serverId}";
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await _db.HashSetAsync(key, new HashEntry[] { new("lastUpdated", now), new("onlineUsers", status.OnlineUsers), new("requestCount", status.RequestCount), new("version", status.Version) });
            // 서버 생존 시간 갱신 (3초 TTL)
            await _db.KeyExpireAsync(key, TimeSpan.FromSeconds(3));

        }
        public async Task<List<string>> GetServerIdsAsync(CancellationToken ct = default)
        {
            var ids = new List<string>();
            var serverKeyPrefix = "server:status:";

            var server = _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints().First());

            // 패턴으로 모든 서버 상태 key 찾기
            foreach (var key in server.Keys(pattern: $"{serverKeyPrefix}*"))
            {
                // key = "server:status:game01"
                var fullKey = key.ToString();

                // serverId = "game01" 추출
                var serverId = fullKey.Substring(serverKeyPrefix.Length);
                ids.Add(serverId);
            }

            return ids;
        }
        public async Task<ServerStatusInfo?> GetServerStatusAsync(string serverId, CancellationToken ct = default)
        {
            var key = $"server:status:{serverId}";

            // TTL 기반이므로 key가 없으면 Dead
            if (!await _db.KeyExistsAsync(key))
                return null;

            var entries = await _db.HashGetAllAsync(key);
            if (entries.Length == 0)
                return null;

            var dict = entries.ToDictionary(
                x => x.Name.ToString(),
                x => x.Value.ToString()
            );

            var lastUpdated = long.Parse(dict["lastUpdated"]);
            var onlineUsers = int.Parse(dict["onlineUsers"]);
            var requestCount = long.Parse(dict["requestCount"]);
            var version = dict["version"];

            // 순서를 올바르게 배치함
            var status = new ServerStatus(
                RequestCount: requestCount,
                OnlineUsers: onlineUsers,
                Version: version
            );

            return new ServerStatusInfo(
                ServerId: serverId,
                Alive: true,
                Status: status,
                LastUpdated: lastUpdated
            );
        }
        public async Task<List<ServerStatusInfo>> GetAllServersAsync(CancellationToken ct = default)
        {
            var result = new List<ServerStatusInfo>();
            var ids = await GetServerIdsAsync(ct);

            foreach (var id in ids)
            {
                var info = await GetServerStatusAsync(id, ct);
                if (info != null)
                    result.Add(info);
            }

            return result;
        }
    }
}
