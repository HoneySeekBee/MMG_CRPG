using Application.Character;
using Application.CharacterModels;
using Application.Contents.Battles;
using Application.Contents.Chapters;
using Application.Contents.Stages;
using Application.Elements;
using Application.EquipSlots;
using Application.Factions;
using Application.Icons;
using Application.Items;
using Application.ItemTypes;
using Application.Monsters;
using Application.Portraits;
using Application.Rarities;
using Application.Roles;
using Application.Skills;
using Infrastructure.Caching;

namespace WebServer.HostedServices
{
    // 정적인 데이터들에만 사용 : 우선은 이렇게 두고 이후에 운영툴에서 아이템 수정 시 반영 등의 작업을 하며 조금 수정해보자. 
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

            await sp.GetRequiredService<IBattlesCache>().ReloadAsync(ct);
            await sp.GetRequiredService<IChapterCache>().ReloadAsync(ct);
            await sp.GetRequiredService<IStagesCache>().ReloadAsync(ct);

            await sp.GetRequiredService<IMonsterCache>().ReloadAsync(ct);

        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
