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
            var sp = scope.ServiceProvider;
            await sp.GetRequiredService<IItemTypeCache>().ReloadAsync(ct);
            await sp.GetRequiredService<IIconCache>().ReloadAsync(ct);
            await sp.GetRequiredService<IPortraitsCache>().ReloadAsync(ct);
            await sp.GetRequiredService<IItemCache>().ReloadAsync(ct);

            await sp.GetRequiredService<IRarityCache>().ReloadAsync(ct);
            await sp.GetRequiredService<IElementCache>().ReloadAsync(ct);
            await sp.GetRequiredService<IRoleCache>().ReloadAsync(ct);
            await sp.GetRequiredService<IFactionCache>().ReloadAsync(ct);
            
            await sp.GetRequiredService<ISkillCache>().ReloadAsync(ct);

            await sp.GetRequiredService<ICharacterCache>().ReloadAsync(ct);
            await sp.GetRequiredService<ICharacterExpCache>().ReloadAsync(ct);
            await sp.GetRequiredService<ICharacterModelCache>().ReloadAsync(ct);

            await sp.GetRequiredService<IEquipSlotCache>().ReloadAsync(ct);

        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
