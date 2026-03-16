using StackExchange.Redis;

namespace Infrastructure.Services.Combat
{
    public sealed class CombatRouteStore
    {
        private static readonly TimeSpan RouteTtl = TimeSpan.FromHours(2);

        private readonly IDatabase _db;

        public CombatRouteStore(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SaveAsync(long combatId, string serverUrl, CancellationToken ct = default)
        {
            await _db.StringSetAsync(
                key: $"combat:route:{combatId}",
                value: serverUrl,
                expiry: RouteTtl);
        }

        public async Task<string?> GetAsync(long combatId, CancellationToken ct = default)
        {
            return await _db.StringGetAsync($"combat:route:{combatId}");
        }

        public async Task DeleteAsync(long combatId, CancellationToken ct = default)
        {
            await _db.KeyDeleteAsync($"combat:route:{combatId}");
        }
    }
}
