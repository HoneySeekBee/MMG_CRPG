using Application.Common.Interface;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class RedisEventStreamLogger : IEventStreamLogger
    {
        private readonly IDatabase _db;
        public RedisEventStreamLogger(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task LogAsync(string stream, Dictionary<string, string> data)
        {
            var entries = data
                .Select(k => new NameValueEntry(k.Key, k.Value))
                .ToArray();

            await _db.StreamAddAsync(stream, entries);
        }

        public async Task<List<StreamEntryDto>> ReadRecentAsync(string stream, int count, CancellationToken ct = default)
        {
            var entries = await _db.StreamRangeAsync(
                key: stream,
                minId: "-",
                maxId: "+",
                count: count,
                messageOrder: Order.Descending 
            );

            return entries.Select(e =>
                new StreamEntryDto(
                    Id: e.Id.ToString(),
                    Fields: e.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString())
                )
            ).ToList();
        }
    }
}
