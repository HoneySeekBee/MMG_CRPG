using Application.Gacha;

namespace WebServer.HostedServices
{
    public sealed class GachaCacheWarmupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<GachaCacheWarmupService> _log;
        public GachaCacheWarmupService(
       IServiceScopeFactory scopes,
       ILogger<GachaCacheWarmupService> log)
        {
            _scopes = scopes;
            _log = log;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.LogInformation("GachaCacheWarmupService started. Preloading Redis...");

            using (var scope = _scopes.CreateScope())
            {
                var cache = scope.ServiceProvider.GetRequiredService<IGachaCacheService>();
                await Warmup(cache, stoppingToken);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                using (var scope = _scopes.CreateScope())
                {
                    var cache = scope.ServiceProvider.GetRequiredService<IGachaCacheService>();
                    await Warmup(cache, stoppingToken);
                }
            }
        }

        private async Task Warmup(IGachaCacheService cache, CancellationToken ct)
        {
            try
            {
                await cache.RefreshAllAsync(ct);
                _log.LogInformation("Gacha cache refreshed.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to refresh gacha cache.");
            }
        }
    }
}