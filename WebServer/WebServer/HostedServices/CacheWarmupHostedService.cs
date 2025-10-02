using Application.Icons;
using Application.Items;
using Application.ItemTypes;
using Application.Portraits;
using Infrastructure.Caching;

namespace WebServer.HostedServices
{
    public sealed class CacheWarmupHostedService : IHostedService
    {
        private readonly IServiceProvider _sp;
        public CacheWarmupHostedService(IServiceProvider sp) => _sp = sp;

        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IItemTypeCache>().ReloadAsync(ct);
            await scope.ServiceProvider.GetRequiredService<IIconCache>().ReloadAsync(ct);
            await scope.ServiceProvider.GetRequiredService<IPortraitsCache>().ReloadAsync(ct);
            await scope.ServiceProvider.GetRequiredService<IItemCache>().ReloadAsync();
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
