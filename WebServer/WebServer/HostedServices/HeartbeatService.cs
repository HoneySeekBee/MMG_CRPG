using Application.Common.Interface;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting.Server;
using StackExchange.Redis;
using System.Net;
using WebServer.Monitoring;

namespace WebServer.HostedServices
{
    public class HeartbeatService : BackgroundService
    {
        private readonly IServerStatusTracker _tracker;
        private readonly IDatabase _db;
        private readonly IConnectionMultiplexer _conn;

        private readonly string _serverId;
        private readonly string _version;

        public HeartbeatService(IServerStatusTracker tracker, IConnectionMultiplexer redis, IConfiguration config)
        {
            _tracker = tracker;
            _db = redis.GetDatabase();
            _conn = redis;

            _serverId =
          Environment.GetEnvironmentVariable("SERVER_ID") ??
          Environment.MachineName ??
          Dns.GetHostName() ??
          Guid.NewGuid().ToString("N");

            _version = config["ServerConfig:Version"] ?? "unknown";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var server = _conn.GetServer(_conn.GetEndPoints()[0]);
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var onlineCount = server.Keys(pattern: "user:online:*").Count();
                var status = new ServerStatus(
                    RequestCount: ServerMetrics.RequestCount,
                     OnlineUsers: onlineCount,
                    Version : _version
                );

                await _tracker.UpdateHeartbeatAsync(_serverId, status);
                
                await _db.StreamAddAsync(
             $"server:history:{_serverId}",
             new NameValueEntry[]
             {
                new("onlineUsers", onlineCount.ToString()),
                new("requestCount", status.RequestCount.ToString()),
                new("version", status.Version),
                new("ts", now.ToString())
             }
         );
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
