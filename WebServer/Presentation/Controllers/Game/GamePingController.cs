using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using WebServer.Utils;

namespace Presentation.Controllers.Game
{
    [Authorize]
    [ApiController]
    [Route("api/pb/ping")]
    [Consumes("application/x-protobuf")]
    [Produces("application/x-protobuf")]
    public class GamePingController : ControllerBase
    {
        private readonly IDatabase _db;

        public GamePingController(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        [HttpPost]
        public async Task<ActionResult<Empty>> Ping([FromBody] Empty req, CancellationToken ct)
        {
            var userId = User.GetUserId();

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
