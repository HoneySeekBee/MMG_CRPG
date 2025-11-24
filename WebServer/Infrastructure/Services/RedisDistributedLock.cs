using Application.Common.Interface;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public sealed class RedisDistributedLock : IDistributedLock
    {
        private readonly IDatabase _redis;

        public RedisDistributedLock(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task<bool> AcquireAsync(string key, TimeSpan expiry)
        { 
            return await _redis.StringSetAsync(
                key,
                "1",
                expiry,
                when: When.NotExists
            );
        }

        public async Task ReleaseAsync(string key)
        {
            await _redis.KeyDeleteAsync(key);
        }
    }
}
