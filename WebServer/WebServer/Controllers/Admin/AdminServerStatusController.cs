using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/servers")]
    [Authorize]
    public class AdminServerStatusController : ControllerBase
    {
        private readonly RedisServerStatusTracker _tracker;

        public AdminServerStatusController(RedisServerStatusTracker tracker)
        {
            _tracker = tracker;
        }
         
        // 서버 목록 + 상태 조회 
        [HttpGet("status")]
        public async Task<IActionResult> GetAllServerStatus(CancellationToken ct)
        {
            var servers = await _tracker.GetAllServersAsync(ct);
            return Ok(servers);
        }

        // 단일 서버 상태 조회
        [HttpGet("{serverId}/status")]
        public async Task<IActionResult> GetServerStatus(string serverId, CancellationToken ct)
        {
            var status = await _tracker.GetServerStatusAsync(serverId, ct);
            if (status == null)
                return NotFound(new { error = "SERVER_NOT_FOUND" });

            return Ok(status);
        }
    }
}
