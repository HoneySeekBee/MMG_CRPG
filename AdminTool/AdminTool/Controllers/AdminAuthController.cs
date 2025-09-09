using AdminTool.Models;
using Application.Users;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AdminTool.Controllers
{
    [Route("admin/auth")]
    public sealed class AdminAuthController : Controller
    {
        private readonly IHttpClientFactory _http;
        public AdminAuthController(IHttpClientFactory http) => _http = http;

        // 공통: GameApi 클라이언트 + 쿠키의 액세스 토큰을 Authorization 헤더로 세팅
        private HttpClient Api()
        {
            var c = _http.CreateClient("GameApi");
            if (Request.Cookies.TryGetValue("accessToken", out var at) && !string.IsNullOrWhiteSpace(at))
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", at);
            return c;
        }

        // ===== Login =====
        [HttpGet("login")]
        public IActionResult Login() => View("~/Views/AdminAuth/Login.cshtml");

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View("~/Views/AdminAuth/Login.cshtml", vm);

            var api = _http.CreateClient("GameApi");
            var resp = await api.PostAsJsonAsync("/api/auth/login",
                new LoginUserRequest(vm.Account, vm.Password), ct);

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await ReadProblemOrRaw(resp, ct));
                return View("~/Views/AdminAuth/Login.cshtml", vm);
            }

            var dto = await resp.Content.ReadFromJsonAsync<LoginResultDto>(cancellationToken: ct);
            if (dto is null)
            {
                ModelState.AddModelError(string.Empty, "로그인 응답 파싱 실패");
                return View("~/Views/AdminAuth/Login.cshtml", vm);
            }

            // 토큰 쿠키 저장 (HttpOnly/Secure)
            Response.Cookies.Append("accessToken", dto.Tokens.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = dto.Tokens.AccessExpiresAt.UtcDateTime
            });
            Response.Cookies.Append("refreshToken", dto.Tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = dto.Tokens.RefreshExpiresAt.UtcDateTime
            });

            TempData["ok"] = "로그인 성공";
            return RedirectToAction("Index", "AdminUsers");
        }

        // ===== Register =====
        [HttpGet("register")]
        public IActionResult Register() => View("~/Views/AdminAuth/Register.cshtml");

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AdminRegisterVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View("~/Views/AdminAuth/Register.cshtml", vm);

            var api = _http.CreateClient("GameApi");
            var resp = await api.PostAsJsonAsync("/api/auth/register",
                new RegisterUserRequest(vm.Account, vm.Password, vm.NickName), ct);

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await ReadProblemOrRaw(resp, ct));
                return View("~/Views/AdminAuth/Register.cshtml", vm);
            }

            TempData["ok"] = "회원이 생성되었습니다. 로그인해 주세요.";
            return RedirectToAction("Login");
        }

        // ===== Logout =====
        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var refresh = Request.Cookies["refreshToken"];
            if (!string.IsNullOrWhiteSpace(refresh))
            {
                var api = Api();
                await api.PostAsJsonAsync("/api/auth/logout", new LogoutRequest(refresh), ct);
            }

            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            TempData["ok"] = "로그아웃 되었습니다.";
            return RedirectToAction("Login");
        }

        // ---- helpers ----
        private static async Task<string> ReadProblemOrRaw(HttpResponseMessage resp, CancellationToken ct)
        {
            try
            {
                var raw = await resp.Content.ReadAsStringAsync(ct);

                try
                {
                    var vpd = JsonSerializer.Deserialize<ValidationProblemDetails>(raw);
                    if (vpd?.Errors?.Count > 0)
                    {
                        var msgs = vpd.Errors.SelectMany(kv => kv.Value).ToArray();
                        return string.Join("\n", msgs);
                    }
                }
                catch { }

                try
                {
                    var pd = JsonSerializer.Deserialize<ProblemDetails>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (!string.IsNullOrWhiteSpace(pd?.Detail)) return pd!.Detail!;
                }
                catch { }

                return $"{(int)resp.StatusCode} {resp.ReasonPhrase} - {raw}";
            }
            catch
            {
                return $"{(int)resp.StatusCode} {resp.ReasonPhrase}";
            }
        }
    }
}
