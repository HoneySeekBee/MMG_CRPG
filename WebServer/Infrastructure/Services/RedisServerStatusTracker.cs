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
            var hbKey = $"server:{serverId}:heartbeat";
            var statusKey = $"server:{serverId}:status";

            // heartbeat: 5초 TTL
            await _db.StringSetAsync(hbKey, "1", TimeSpan.FromSeconds(5));

            // status JSON 기록
            var json = JsonSerializer.Serialize(status);
            await _db.StringSetAsync(statusKey, json);

            // 서버 목록 등록
            if (string.IsNullOrWhiteSpace(serverId))
                throw new ArgumentException("serverId cannot be null or empty");
            await _db.SetAddAsync("server:list", serverId);
        }

        public async Task<List<string>> GetServerIdsAsync(CancellationToken ct = default)
        {
            var ids = await _db.SetMembersAsync("server:list");
            return ids.Select(x => (string)x).ToList();
        }

        public async Task<ServerStatusInfo?> GetServerStatusAsync(string serverId, CancellationToken ct = default)
        {
            var hb = await _db.StringGetAsync($"server:{serverId}:heartbeat");
            var json = await _db.StringGetAsync($"server:{serverId}:status");

            if (json.IsNullOrEmpty) return null;

            var status = JsonSerializer.Deserialize<ServerStatus>(json!)!;
            var alive = hb.HasValue;

            return new ServerStatusInfo(
                ServerId: serverId,
                Alive: alive,
                Status: status,
                LastUpdated: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
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
