using Application.Contents.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public interface ICombatService
    {
        Task<StartCombatResponse> StartAsync(StartCombatRequest req, CancellationToken ct);

        Task EnqueueCommandAsync(long combatId, CombatCommandDto cmd, CancellationToken ct);

        Task<SimulateCombatResponse> SimulateAsync(
            SimulateCombatRequest request, CancellationToken ct);

        Task<CombatLogPageDto> GetLogAsync(
            long combatId, string? cursor, int size, CancellationToken ct);

        Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct);
        Task<CombatTickResponse> TickAsync(long combatId, int tick, CancellationToken ct);

        // 선택: 보상 수령 등
        // Task<ClaimResponse> ClaimAsync(long combatId, CancellationToken ct);
    }
    public interface IStageReader { Task<StageDetailDto> GetAsync(long stageId, CancellationToken ct); }
    public interface ICharacterReader { Task<CharacterMasterDto> GetAsync(long characterId, CancellationToken ct); }
    public interface ISkillReader { Task<SkillMasterDto> GetAsync(long skillId, CancellationToken ct); }

    // 공용 포트(선택)
    public interface ITimeProvider { DateTimeOffset UtcNow { get; } }
    public interface IRandomProvider { Random Create(long seed); }
}
