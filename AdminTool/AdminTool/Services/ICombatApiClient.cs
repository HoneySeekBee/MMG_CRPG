using Application.Combat;

namespace AdminTool.Services
{
    public interface ICombatApiClient
    {
        Task<SimulateCombatResponse> SimulateAsync(SimulateCombatRequest req, CancellationToken ct);
        Task<CombatLogPageDto> GetLogAsync(long combatId, string? cursor, int size, CancellationToken ct);
        Task<CombatLogSummaryDto> GetSummaryAsync(long combatId, CancellationToken ct);
    }
}
