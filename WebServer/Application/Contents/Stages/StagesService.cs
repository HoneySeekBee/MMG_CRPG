using Application.Common.Models;
using Application.Repositories;
using Domain.Entities.Contents;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Stages
{
    public sealed class StagesService : IStagesService
    {
        private readonly IStagesRepository _repo;
        private readonly IStageQueryRepository _qrepo;
        private readonly ISecurityEventSink _audit;
        private readonly ILogger<StagesService> _log;

        public StagesService(
            IStagesRepository repo,
            IStageQueryRepository qrepo,
            ISecurityEventSink audit,
            ILogger<StagesService> log)
        {
            _repo = repo;
            _qrepo = qrepo;
            _audit = audit;
            _log = log;
        }

        public Task<Common.Models.PagedResult<StageSummaryDto>> GetListAsync(StageListFilter filter, CancellationToken ct)
            => _qrepo.GetListAsync(filter, ct);  // 반환 타입 일치

        public Task<StageDetailDto?> GetDetailAsync(int id, CancellationToken ct)
            => _qrepo.GetDetailAsync(id, ct);

        public async Task<int> CreateAsync(CreateStageRequest req, CancellationToken ct)
        {
            // 중복 체크
            if (await _repo.ExistsChapterOrderAsync(req.Chapter, req.StageNumer, null, ct))
                throw new InvalidOperationException("DUPLICATE_CHAPTER_ORDER");

            var e = ToEntity(req);
            e.Validate();

            await _repo.AddAsync(e, ct);
            await _repo.SaveAsync(ct);

            await _audit.LogAsync("StageChanged", null, new { Action = "Create", StageId = e.Id }, ct);
            _log.LogInformation("Stage created {StageId}", e.Id);
            return e.Id;
        }

        public async Task UpdateAsync(int id, UpdateStageRequest req, CancellationToken ct)
        {
            var current = await _repo.LoadGraphAsync(id, ct) ?? throw new InvalidOperationException("STAGE_NOT_FOUND");

            if (await _repo.ExistsChapterOrderAsync(req.Chapter, req.StageNumer, id, ct))
                throw new InvalidOperationException("DUPLICATE_CHAPTER_ORDER");

            Apply(current, req);     // 그래프 재구성
            current.Validate();

            await _repo.SaveAsync(ct);
            await _audit.LogAsync("StageChanged", null, new { Action = "Update", StageId = id }, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var graph = await _repo.LoadGraphAsync(id, ct) ?? throw new InvalidOperationException("STAGE_NOT_FOUND");
            _repo.Remove(graph);
            await _repo.SaveAsync(ct);

            await _audit.LogAsync("StageChanged", null, new { Action = "Delete", StageId = id }, ct);
        }

        // ------- Req -> Domain 매핑 (필요부분만) -------
        private static Stage ToEntity(CreateStageRequest x)
        {
            var s = new Stage(x.Chapter, x.StageNumer, x.RecommendedPower, x.StaminaCost, x.IsActive);

            AttachChildren(s, x.Waves, x.Drops, x.FirstRewards, x.Requirements, x.Batches);
            return s;
        }

        private static void Apply(Stage s, UpdateStageRequest x)
        {
            s.SetBasic(x.Chapter, x.StageNumer, x.RecommendedPower, x.StaminaCost, x.IsActive);

            s.Waves.Clear(); 
            s.Drops.Clear(); 
            s.FirstRewards.Clear(); 
            s.Requirements.Clear();
            s.Batches.Clear();

            AttachChildren(s, x.Waves, x.Drops, x.FirstRewards, x.Requirements, x.Batches);
        }

        private static void AttachChildren(Stage s,
            IReadOnlyList<WaveCmd> waves,
            IReadOnlyList<DropCmd> drops,
            IReadOnlyList<RewardCmd> rewards,
            IReadOnlyList<RequirementCmd> reqs,
            IReadOnlyList<BatchCmd> batches)
        {
            foreach (var w in waves.OrderBy(_ => _.Index))
            {
                var wave = new StageWave(w.Index);
                foreach (var e in w.Enemies.OrderBy(_ => _.Slot))
                    wave.Enemies.Add(new StageWaveEnemy(e.EnemyCharacterId, e.Level, e.Slot, e.AiProfile));
                s.Waves.Add(wave);
            }
            foreach (var d in drops) s.Drops.Add(new StageDrop(d.ItemId, d.Rate, d.MinQty, d.MaxQty, d.FirstClearOnly));
            foreach (var r in rewards) s.FirstRewards.Add(new StageFirstClearReward(r.ItemId, r.Qty));
            foreach (var r in reqs) s.Requirements.Add(new StageRequirement(r.RequiredStageId, r.MinAccountLevel)); 
            foreach (var b in batches.OrderBy(_ => _.Index))
                s.Batches.Add(new StageBatch(b.Index, b.AssetKey));
        }
    }
}
