using Application.Contents.Stages;
using Domain.Enum;
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
        Task<CombatLogPageDto> GetLogAsync(long combatId, string? cursor, int size, CancellationToken ct);
        Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct);
        Task<CombatTickResponse> TickAsync(long combatId, int tick, CancellationToken ct);
        Task<CombatSpeed> ToggleSpeedAsync(long combatId, CancellationToken ct);
    }

    public interface IStageReader
    {
        Task<StageDetailDto?> GetAsync(int stageId, CancellationToken ct);
    }

    public interface ICharacterReader
    {
        Task<CharacterMasterDto?> GetAsync(long characterId, CancellationToken ct);
    }

    public interface ISkillReader
    {
        Task<SkillMasterDto?> GetAsync(long skillId, CancellationToken ct);
    }

    public interface ITimeProvider { DateTimeOffset UtcNow { get; } }
    public interface IRandomProvider { Random Create(long seed); }
}
