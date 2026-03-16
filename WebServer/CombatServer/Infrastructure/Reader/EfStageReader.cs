using Application.Combat;
using Application.Contents.Stages;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Reader
{
    public sealed class EfStageReader : IStageReader
    {
        private readonly CombatDbContext _db;

        public EfStageReader(CombatDbContext db) => _db = db;

        public async Task<StageDetailDto?> GetAsync(int stageId, CancellationToken ct)
        {
            var stage = await _db.StageRows
                .Where(s => s.Id == stageId)
                .FirstOrDefaultAsync(ct);

            if (stage is null)
                return null;

            var waves = await _db.StageWaveRows
                .Where(w => w.StageId == stageId)
                .ToListAsync(ct);

            var waveIds = waves.Select(w => w.Id).ToList();

            var enemies = await _db.StageWaveEnemyRows
                .Where(e => waveIds.Contains(e.StageWaveId))
                .ToListAsync(ct);

            var waveDtos = waves
                .OrderBy(w => w.Index)
                .Select(w => new WaveDto(
                    w.Index,
                    enemies
                        .Where(e => e.StageWaveId == w.Id)
                        .Select(e => new EnemyDto(e.EnemyCharacterId, e.Level, e.Slot, e.AiProfile))
                        .ToList(),
                    w.BatchNum))
                .ToList();

            return new StageDetailDto(
                stage.Id,
                stage.Chapter,
                stage.StageNum,
                stage.Name,
                stage.RecommendedPower,
                stage.StaminaCost,
                stage.IsActive,
                waveDtos,
                Array.Empty<DropDto>(),
                Array.Empty<RewardDto>(),
                Array.Empty<RequirementDto>(),
                Array.Empty<BatchDto>());
        }
    }
}
