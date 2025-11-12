using AdminTool.Models;
using Application.Common.Models;
using Application.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using CommonModels = Application.Common.Models;

namespace AdminTool.Controllers
{
    [Route("admin/security")]
    public sealed class AdminSecurityController : Controller
    {
        private readonly IHttpClientFactory _http;
        public AdminSecurityController(IHttpClientFactory http) => _http = http;

        private HttpClient Api()
        {
            var c = _http.CreateClient("GameApi");
            if (Request.Cookies.TryGetValue("accessToken", out var at) && !string.IsNullOrWhiteSpace(at))
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", at);
            return c;
        }

        // 메인 → 이벤트
        [HttpGet("")]
        public IActionResult Index() => RedirectToAction(nameof(Events));

        // 이벤트 목록
        [HttpGet("events")]
        public async Task<IActionResult> Events([FromQuery] SecurityEventSearchVm q, CancellationToken ct)
        {
            var api = Api();

            var url = QueryHelpers.AddQueryString("/api/admin/security/events", new Dictionary<string, string?>
            {
                ["userId"] = q.UserId?.ToString(),
                ["type"] = q.Type, // 예: LoginSuccess / LoginFail / TokenRefresh / Logout
                ["from"] = q.From?.ToString("o"),
                ["to"] = q.To?.ToString("o"),
                ["page"] = q.Page.ToString(),
                ["pageSize"] = q.PageSize.ToString()
            });

            var page = await api.GetFromJsonAsync<CommonModels.PagedResult<SecurityEventBriefDto>>(url, ct)
           ?? new CommonModels.PagedResult<SecurityEventBriefDto>(
                Items: Array.Empty<SecurityEventBriefDto>(),
                Page: q.Page,
                PageSize: q.PageSize,
                TotalCount: 0
              );
            var vm = page.ToVm(q); // SecurityEventListVm 로 매핑 (앞서 만든 확장 메서드)
            return View("~/Views/AdminSecurity/Events.cshtml", vm);
        }
    }
}
