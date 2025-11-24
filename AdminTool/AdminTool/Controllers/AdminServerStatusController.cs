using AdminTool.Models;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace AdminTool.Controllers
{
    [Route("admin/servers")]
    public class AdminServerStatusController : Controller
    {
        private readonly IHttpClientFactory _http;

        public AdminServerStatusController(IHttpClientFactory http)
        {
            _http = http;
        }
        private HttpClient Api()
        {
            var c = _http.CreateClient("GameApi");
            return c;    
        }
        // 메인 → 상태 페이지
        [HttpGet("")]
        public IActionResult Index() => RedirectToAction(nameof(Status));

        // 전체 서버 상태
        [HttpGet("status")]
        public async Task<IActionResult> Status(CancellationToken ct)
        {
            var api = Api();

            var list = await api.GetFromJsonAsync<List<ServerStatusInfoDto>>(
                "/api/admin/servers/status", ct)
                ?? new List<ServerStatusInfoDto>();

            var vm = new AdminServerStatusVm { Servers = list };
            return View("~/Views/AdminServerStatus/Status.cshtml", vm);
        }

        // 단일 서버 상세
        [HttpGet("{serverId}")]
        public async Task<IActionResult> Detail(string serverId, CancellationToken ct)
        {
            var api = Api();

            var dto = await api.GetFromJsonAsync<ServerStatusInfoDto>(
                $"/api/admin/servers/{serverId}/status", ct);

            if (dto == null)
                return NotFound();

            var vm = new AdminServerDetailVm
            {
                ServerId = dto.ServerId,
                Alive = dto.Alive,
                LastUpdated = dto.LastUpdated,
                Version = dto.Status?.Version ?? "-",
                Region = dto.Status?.Region ?? "-",
                OnlineUsers = dto.Status?.OnlineUsers ?? 0,
                RequestsPerSec = dto.Status?.RequestsPerSec ?? 0,
                RequestCount = dto.Status?.RequestCount ?? 0
            };

            return View("~/Views/AdminServerStatus/Detail.cshtml", vm);
        }
        public sealed class ServerStatusInfoDto
        {
            public string ServerId { get; set; } = "";
            public bool Alive { get; set; }
            public long LastUpdated { get; set; }
            public ServerStatusDto? Status { get; set; }
        }

        public sealed class ServerStatusDto
        {
            public string Version { get; set; } = "";
            public string Region { get; set; } = "";
            public int OnlineUsers { get; set; }
            public int RequestsPerSec { get; set; }
            public int RequestCount { get; set; }
        }
    }
}
