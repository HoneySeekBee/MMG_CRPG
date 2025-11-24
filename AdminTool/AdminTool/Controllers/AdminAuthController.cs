using AdminTool.Models;
using Application.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace AdminTool.Controllers
{
    [Route("admin/auth")]
    public sealed class AdminAuthController : Controller
    {
        private readonly IHttpClientFactory _http;
        public AdminAuthController(IHttpClientFactory http) => _http = http;

        // 공통: GameApi 클라이언트 + 쿠키의 액세스 토큰을 Authorization 헤더로 세팅
        private HttpClient Api() => _http.CreateClient("GameApi");
        // ===== Login =====
        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult Login([FromQuery] string? returnUrl = null)
        {
            return View("~/Views/AdminAuth/Login.cshtml",
                new AdminLoginVm { ReturnUrl = returnUrl });
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginVm vm, CancellationToken ct)
        {
            Console.WriteLine($"1 Auth의 Return : {vm.ReturnUrl} ");
            if (!ModelState.IsValid) return View("~/Views/AdminAuth/Login.cshtml", vm);

            // WebAPI에 로그인 요청
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

            // 세션에 토큰 저장 
            HttpContext.Session.SetString("access_token", dto.Tokens.AccessToken);
            HttpContext.Session.SetString("refresh_token", dto.Tokens.RefreshToken);

            // 쿠키 세션 발급 
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, dto.User.Id.ToString()),
        new Claim(ClaimTypes.Name, dto.User.Account),
        // 필요하면 역할/권한
        // new Claim(ClaimTypes.Role, "admin"),
    };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            TempData["ok"] = "로그인 성공";

            Console.WriteLine($"Auth의 Return : {vm.ReturnUrl} ");

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

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
            // (1) 웹서버에 로그아웃 알림
            var refresh = HttpContext.Session.GetString("refresh_token");
            if (!string.IsNullOrWhiteSpace(refresh))
            {
                var api = _http.CreateClient("GameApi");
                try { await api.PostAsJsonAsync("/api/auth/logout", new LogoutRequest(refresh), ct); }
                catch { /* 실패해도 무시 */ }
            }

            // (2) 세션에 로그인 토큰 제거
            HttpContext.Session.Remove("access_token");
            HttpContext.Session.Remove("refresh_token");

            // (3) 운영툴 "쿠키 세션" 종료 
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

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
