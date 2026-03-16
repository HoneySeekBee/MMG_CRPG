using StackExchange.Redis;

namespace Infrastructure.Services.Combat
{
    public sealed class CombatServerSelector
    {
        private readonly IConnectionMultiplexer _redis;

        public CombatServerSelector(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public string SelectServer()
        {
            var endpoint = _redis.GetEndPoints()[0];
            var server = _redis.GetServer(endpoint);

            var keys = server.Keys(pattern: "combat:server:*").ToArray();
            if (keys.Length == 0)
                throw new InvalidOperationException("No CombatServer instances are registered in Redis.");

            var db = _redis.GetDatabase();

            // Pick a random key to distribute load evenly
            var key = keys[Random.Shared.Next(keys.Length)];
            var url = (string?)db.StringGet(key);

            if (string.IsNullOrEmpty(url))
                throw new InvalidOperationException($"CombatServer key '{key}' has no URL value.");

            return url;
        }
    }
}
