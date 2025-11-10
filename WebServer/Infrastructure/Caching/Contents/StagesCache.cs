using Application.Contents.Stages;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching.Contents
{
    public class StagesCache : IStagesCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        private readonly object _gate = new();

        private List<StageDetailDto> _all = new();
        private Dictionary<int, StageDetailDto> _byId = new();

        public StagesCache(IDbContextFactory<GameDBContext> factory)
        {
            _factory = factory;
        }

        public IReadOnlyList<StageDetailDto> GetAll() => _all;

        public StageDetailDto? GetById(int id) =>
            _byId.TryGetValue(id, out var v) ? v : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);
            
            var stages = await db.Stages
    .AsNoTracking()
    .Include(s => s.Waves).ThenInclude(w => w.Enemies)
    .Include(s => s.Drops)
    .Include(s => s.FirstRewards)
    .Include(s => s.Requirements)
    .Include(s => s.Batches)
    .ToListAsync(ct);

            var list = stages.Select(s => s.ToDetailDto()).ToList();
            var byId = list.ToDictionary(x => x.Id);

            lock (_gate)
            {
                _all = list;
                _byId = byId;
            }

            Console.WriteLine($"[StagesCache] loaded: {_all.Count}");
        }
        private static StageDetailDto ToDetailDto(Domain.Entities.Contents.Stage s)
        {
            return new StageDetailDto(
                Id: s.Id,
                Chapter: s.Chapter,
                Order: s.StageNumber,                   
                Name: s.Name,
                RecommendedPower: s.RecommendedPower,
                StaminaCost: s.StaminaCost,
                IsActive: s.IsActive,
                Waves: s.Waves
                    .OrderBy(w => w.Index)
                    .Select(w => new WaveDto(
                        w.Index,
                        w.Enemies
                            .OrderBy(e => e.Slot)
                            .Select(e => new EnemyDto(
                                e.EnemyCharacterId,
                                e.Level,
                                e.Slot,
                                e.AiProfile
                            ))
                            .ToList()
                    ))
                    .ToList(),
                Drops: s.Drops
                    .Select(d => new DropDto(
                        d.ItemId,
                        d.Rate,
                        d.MinQty,
                        d.MaxQty,
                        d.FirstClearOnly
                    ))
                    .ToList(),
                FirstRewards: s.FirstRewards
                    .Select(r => new RewardDto(
                        r.ItemId,
                        r.Qty
                    ))
                    .ToList(),
                Requirements: s.Requirements
                    .Select(r => new RequirementDto(
                        r.RequiredStageId,
                        r.MinAccountLevel
                    ))
                    .ToList(),
                Batches: s.Batches
                    .OrderBy(b => b.BatchNum)
                    .Select(b => new BatchDto(
                        b.BatchNum,
                        b.UnitKey,
                        b.EnvKey
                    ))
                    .ToList()
            );
        }
    } 
}
