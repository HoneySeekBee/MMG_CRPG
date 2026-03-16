using Application.Combat.Snapshot;

namespace Application.Combat
{
    public interface ICombatStateCache
    {
        Task SaveAsync(CombatStateSnapshot snapshot, CancellationToken ct = default);
        Task<CombatStateSnapshot?> LoadAsync(long combatId, CancellationToken ct = default);
        Task DeleteAsync(long combatId, CancellationToken ct = default);
    }
}
