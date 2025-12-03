using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using WebServer.Utils;

namespace WebServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/ping")]
    public class PingController : ControllerBase
    {
        private readonly IDatabase _db;

        public PingController(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        [HttpPost]
        public async Task<IActionResult> Ping(CancellationToken ct)
        { 
            var userId = User.GetUserId(); // Claims에서 가져오기 
            if (User.IsInRole("admin"))
                return Ok(new { ok = true });

            string key = $"user:online:{userId}";

            // TTL 6초
            await _db.StringSetAsync(key, "1", TimeSpan.FromSeconds(6));

            return Ok(new { ok = true });
        }
    }
}
