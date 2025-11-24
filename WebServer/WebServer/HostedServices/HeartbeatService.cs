using Application.Common.Interface;
using Domain.Common;
using System.Net;
using WebServer.Monitoring;

namespace WebServer.HostedServices
{
    public class HeartbeatService : BackgroundService
    {
        private readonly IServerStatusTracker _tracker;
        private readonly string _serverId;

        public HeartbeatService(IServerStatusTracker tracker, IConfiguration config)
        {
            _tracker = tracker;
            _serverId =
          Environment.GetEnvironmentVariable("SERVER_ID") ??
          Environment.MachineName ??
          Dns.GetHostName() ??
          Guid.NewGuid().ToString("N");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var status = new ServerStatus(
                    RequestCount: ServerMetrics.RequestCount,
                    OnlineUsers: ServerMetrics.OnlineUserCount
                );

                await _tracker.UpdateHeartbeatAsync(_serverId, status);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
