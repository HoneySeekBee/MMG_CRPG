using Application.Combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface ICombatRepository
    {
        Task<long> SaveAsync(Domain.Entities.Combat combat,
            IEnumerable<Domain.Events.CombatLogEvent> events,
            CancellationToken ct);

        Task<CombatLogPageDto> GetLogAsync(
            long combatId, string? cursor, int size, CancellationToken ct);
        Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct);
    }
}
