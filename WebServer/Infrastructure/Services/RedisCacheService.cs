using Application.Common.Interface;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }
        public Task SetAsync(string key, string value, TimeSpan? exp = null)
        {
            if (exp.HasValue)
                return _db.StringSetAsync(key, value, expiry: exp.Value);

            return _db.StringSetAsync(key, value);
        }

        public async Task<string?> GetAsync(string key)
        {
            var result = await _db.StringGetAsync(key);
            return result.HasValue ? result.ToString() : null;
        }

        public Task RemoveAsync(string key)
        {
            return _db.KeyDeleteAsync(key);
        }
    }
}
