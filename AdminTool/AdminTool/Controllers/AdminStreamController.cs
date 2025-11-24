using AdminTool.Models;
using Application.Common.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace AdminTool.Controllers
{
    [Route("admin/streams")]
    public class AdminStreamController : Controller
    {
        private readonly IHttpClientFactory _http;

        public AdminStreamController(IHttpClientFactory http)
        {
            _http = http;
        }
        private HttpClient Api() => _http.CreateClient("GameApi");

        // 기본 → user-events 조회
        [HttpGet("")]
        public IActionResult Index()
            => RedirectToAction(nameof(Stream), new { stream = "stream:user-events" });

        [HttpGet("{stream}")]
        public async Task<IActionResult> Stream(string stream, int count = 50, CancellationToken ct = default)
        {
            var api = Api();
            var url = $"/api/admin/streams/{stream}?count={count}";

            // WebAPI DTO 받아오기
            var entries = await api.GetFromJsonAsync<List<StreamEntryDto>>(url, ct)
                          ?? new List<StreamEntryDto>();

            // DTO → VM 변환
            var vm = entries.Select(e => new AdminStreamEntryVm
            {
                Id = e.Id,
                Fields = e.Fields,
                TimestampLocal = ConvertTimestamp(e)
            }).ToList();

            ViewBag.StreamName = stream;
            return View("~/Views/AdminStream/Stream.cshtml", vm);
        }
        private string ConvertTimestamp(StreamEntryDto dto)
        {
            // StreamEntryDto에도 timestamp가 들어가 있는 경우(직접 넣은 필드)
            if (dto.Fields.TryGetValue("timestamp", out var ts))
            {
                if (DateTimeOffset.TryParse(ts, out var dt))
                    return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            }

            // 스트림 ID 자체에서 UNIX MS가 들어있는 경우
            var parts = dto.Id.Split('-');
            if (parts.Length > 0 && long.TryParse(parts[0], out var ms))
            {
                try
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(ms)
                        .ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch { }
            }

            // fallback
            return "-";
        }

    }
}
