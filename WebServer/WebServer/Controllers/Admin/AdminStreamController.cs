using Application.Common.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/streams")]
    [Authorize]
    public class AdminStreamController : ControllerBase
    {
        private readonly IEventStreamLogger _logger;

        public AdminStreamController(IEventStreamLogger logger)
        {
            _logger = logger;
        }

        // Redis Stream 조회
        // ex) /api/admin/streams/user-events?count=50
        [HttpGet("{streamName}")]
        public async Task<IActionResult> ReadEvents(
            string streamName,
            [FromQuery] int count = 100,
            CancellationToken ct = default)
        {
            var events = await _logger.ReadRecentAsync(streamName, count, ct);
            return Ok(events);
        }
    }
}
