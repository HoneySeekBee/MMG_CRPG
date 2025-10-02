using Application.Elements;
using Application.Factions;
using Application.Icons;
using Application.Items;
using Application.ItemTypes;
using Application.Portraits;
using Application.Rarities;
using Application.Roles;
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

            await scope.ServiceProvider.GetRequiredService<IRarityCache>().ReloadAsync();
            await scope.ServiceProvider.GetRequiredService<IElementCache>().ReloadAsync();
            await scope.ServiceProvider.GetRequiredService<IRoleCache>().ReloadAsync();
            await scope.ServiceProvider.GetRequiredService<IFactionCache>().ReloadAsync();

        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
