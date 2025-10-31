using Application.Character;
using Application.CharacterModels;
using Application.Elements;
using Application.EquipSlots;
using Application.Factions;
using Application.Icons;
using Application.Items;
using Application.ItemTypes;
using Application.Portraits;
using Application.Rarities;
using Application.Roles;
using Application.Skills;
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
            await scope.ServiceProvider.GetRequiredService<IItemCache>().ReloadAsync(ct);

            await scope.ServiceProvider.GetRequiredService<IRarityCache>().ReloadAsync(ct);
            await scope.ServiceProvider.GetRequiredService<IElementCache>().ReloadAsync(ct);
            await scope.ServiceProvider.GetRequiredService<IRoleCache>().ReloadAsync(ct);
            await scope.ServiceProvider.GetRequiredService<IFactionCache>().ReloadAsync(ct);
            
            await scope.ServiceProvider.GetRequiredService<ISkillCache>().ReloadAsync(ct);

            await scope.ServiceProvider.GetRequiredService<ICharacterCache>().ReloadAsync(ct);
            await scope.ServiceProvider.GetRequiredService<ICharacterExpCache>().ReloadAsync(ct);
            await scope.ServiceProvider.GetRequiredService<ICharacterModelCache>().ReloadAsync(ct);

            await scope.ServiceProvider.GetRequiredService<IEquipSlotCache>().ReloadAsync(ct);



        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
