using Application.Combat;
using Application.Combat.Snapshot;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Cache
{
    public sealed class CombatStateCache : ICombatStateCache
    {
        private static readonly TimeSpan StateTtl = TimeSpan.FromHours(2);

        private readonly IDatabase _db;

        public CombatStateCache(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SaveAsync(CombatStateSnapshot snapshot, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(snapshot);
            await _db.StringSetAsync(
                key: $"combat:state:{snapshot.CombatId}",
                value: json,
                expiry: StateTtl);
        }

        public async Task<CombatStateSnapshot?> LoadAsync(long combatId, CancellationToken ct = default)
        {
            var json = await _db.StringGetAsync($"combat:state:{combatId}");
            if (json.IsNullOrEmpty) return null;

            return JsonSerializer.Deserialize<CombatStateSnapshot>(json!);
        }

        public async Task DeleteAsync(long combatId, CancellationToken ct = default)
        {
            await _db.KeyDeleteAsync($"combat:state:{combatId}");
        }
    }
}
