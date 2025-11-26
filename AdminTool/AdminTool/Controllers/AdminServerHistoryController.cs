using Microsoft.AspNetCore.Mvc;

namespace AdminTool.Controllers
{
    [Route("admin/servers")]
    public class AdminServerHistoryController : Controller
    {
        private readonly IHttpClientFactory _http;

        public AdminServerHistoryController(IHttpClientFactory http)
        {
            _http = http;
        }

        private HttpClient Api()
        {
            var client = _http.CreateClient("GameApi");

            var access = HttpContext.Session.GetString("access_token");
            if (!string.IsNullOrEmpty(access))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);
            }

            return client;
        }

        // DTO를 명시적으로 선언
        public class HistoryDto
        {
            public long ts { get; set; }
            public int onlineUsers { get; set; }
            public long requestCount { get; set; }
        }

        [HttpGet("{serverId}/history")]
        public async Task<IActionResult> History(string serverId, int seconds = 60, CancellationToken ct = default)
        {
            var api = Api();
            var url = $"/api/admin/servers/{serverId}/history?seconds={seconds}";
             
            var data = await api.GetFromJsonAsync<List<HistoryDto>>(url, ct);

            // 그대로 JSON으로 UI에 전달
            return Json(data);
        }

    }
}
