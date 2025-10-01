using Application.Common.Models;
using Application.UserInventory;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Security.Claims;

namespace AdminTool.Controllers
{
    [Authorize]
    [Route("admin/UserInventory")]
    public class UserInventoryController : Controller
    {
        private readonly IHttpClientFactory _http;
        public UserInventoryController(IHttpClientFactory http) => _http = http;

        // 특정 유저의 인벤토리 조회 (예: /admin/user-inventory?userId=123)
        [HttpGet("")]
        public async Task<IActionResult> Index(int? userId, int page = 1, int pageSize = 50, CancellationToken ct = default)
        {
            int uid;
            if (userId.HasValue && userId.Value > 0)
            {
                uid = userId.Value;
            }
            else
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                uid = int.TryParse(idStr, out var parsed) ? parsed : 0;
            }

            if (uid <= 0)
            {
                TempData["err"] = "userId가 필요합니다.";
                return RedirectToAction("Index", "AdminUsers");
            }


            var api = _http.CreateClient("GameApi");
            var url = QueryHelpers.AddQueryString($"/api/users/{uid}/inventory", // 캐릭터면 /characters
                new Dictionary<string, string?> { ["page"] = page.ToString(), ["pageSize"] = pageSize.ToString() });

            var resp = await api.GetAsync(url, ct);

            if (resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                // 토큰이 없거나 만료 → 운영툴 쿠키 세션 종료 + 로그인으로
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear(); // 서버 세션(JWT)도 비움
                var returnUrl = Url.Action(nameof(Index), new { userId = uid, page, pageSize });
                return RedirectToAction("Login", "AdminAuth", new { returnUrl });
            }

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                return Content($"API {(int)resp.StatusCode} {resp.ReasonPhrase}\n\n{body}", "text/plain; charset=utf-8");
            }

            var data = await resp.Content.ReadFromJsonAsync<PagedResult<UserInventoryDto>>(cancellationToken: ct)
                       ?? new(Array.Empty<UserInventoryDto>(), page, pageSize, 0);

            ViewBag.UserId = uid;

            return View(data);
        }
    }
}