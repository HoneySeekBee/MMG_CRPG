using Application.Combat; 

namespace Application.Repositories
{
    public interface ICombatRepository
    {
        Task<long> SaveAsync(Domain.Entities.Combat combat,
            IEnumerable<Domain.Events.CombatLogEvent> events,
            CancellationToken ct);
        Task AppendLogsAsync(
            long combatId,
            IEnumerable<Domain.Events.CombatLogEvent> events,
            CancellationToken ct);
        Task<CombatLogPageDto> GetLogAsync(long combatId, string? cursor, int size, CancellationToken ct);
        Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct);
    }
}
