using StackExchange.Redis;

namespace CombatServer.HostedServices
{
    public sealed class CombatServerRegistryService : BackgroundService
    {
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan KeyTtl = TimeSpan.FromSeconds(30);

        private readonly IDatabase _db;
        private readonly string _instanceId;
        private readonly string _selfUrl;
        private readonly ILogger<CombatServerRegistryService> _logger;

        public CombatServerRegistryService(
            IConnectionMultiplexer redis,
            IConfiguration config,
            ILogger<CombatServerRegistryService> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;

            _instanceId =
                Environment.GetEnvironmentVariable("INSTANCE_ID") ??
                Environment.MachineName;

            _selfUrl = config["CombatServer:SelfUrl"]
                ?? throw new InvalidOperationException("CombatServer:SelfUrl is not configured.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CombatServer registering as '{InstanceId}' at '{Url}'", _instanceId, _selfUrl);

            while (!stoppingToken.IsCancellationRequested)
            {
                await _db.StringSetAsync(
                    key: $"combat:server:{_instanceId}",
                    value: _selfUrl,
                    expiry: KeyTtl);

                await Task.Delay(HeartbeatInterval, stoppingToken);
            }

            // Deregister on graceful shutdown
            await _db.KeyDeleteAsync($"combat:server:{_instanceId}");
            _logger.LogInformation("CombatServer '{InstanceId}' deregistered from Redis", _instanceId);
        }
    }
}
