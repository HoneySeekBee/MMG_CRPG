using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using WebServer.Utils;

namespace WebServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/pb/ping")]
    [Consumes("application/x-protobuf")]
    [Produces("application/x-protobuf")]
    public class PingProtoController : ControllerBase
    {
        private readonly IDatabase _db;

        public PingProtoController(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        [HttpPost]
        public async Task<ActionResult<Empty>> Ping([FromBody] Empty req, CancellationToken ct)
        {
            var userId = User.GetUserId();
            Console.WriteLine($"[Ping] user={userId}");

            if (userId <= 0)
                return Unauthorized();

            if (User.IsInRole("admin"))
                return Ok(new Empty());

            await _db.StringSetAsync(
                $"user:online:{userId}",
                "1",
                TimeSpan.FromSeconds(6)
            );

            return Ok(new Empty());
        }
    } 
}
