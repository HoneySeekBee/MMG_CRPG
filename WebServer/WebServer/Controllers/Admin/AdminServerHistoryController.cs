using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace WebServer.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/servers")]
    [Authorize]
    public class AdminServerHistoryController : ControllerBase
    {
        private readonly IDatabase _db; 
        public AdminServerHistoryController(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase(); 
        }

        // GET: /api/admin/servers/{serverId}/history?seconds=60
        [HttpGet("{serverId}/history")]
        public async Task<IActionResult> GetHistory(
                string serverId,
                [FromQuery] int seconds = 60,
                CancellationToken ct = default)
        {
            var key = $"server:history:{serverId}";
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var minTimestamp = now - (seconds * 1000);

            var entries = await _db.StreamRangeAsync(
                key,
                minId: minTimestamp.ToString(),
                maxId: "+",
                count: null
            );

            var result = entries
                .Select(e => new
                {
                    ts = long.Parse(e.Values.First(v => v.Name == "ts").Value!),
                    onlineUsers = int.Parse(e.Values.First(v => v.Name == "onlineUsers").Value!),
                    requestCount = long.Parse(e.Values.First(v => v.Name == "requestCount").Value!)
                })
                .OrderBy(x => x.ts)
                .ToList();

            return Ok(result);
        }
    }
}
