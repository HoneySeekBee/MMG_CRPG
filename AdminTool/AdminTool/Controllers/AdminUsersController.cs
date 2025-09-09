using AdminTool.Models;
using Application.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text.Json;
using System.Text;

namespace AdminTool.Controllers
{
    [Route("admin/users")]
    public sealed class AdminUsersController : Controller
    {
        private readonly IHttpClientFactory _http;

        public AdminUsersController(IHttpClientFactory http) => _http = http;

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] UserSearchVm q, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");

            var url = QueryHelpers.AddQueryString("/api/admin/users", new Dictionary<string, string?>
            {
                ["page"] = q.Page.ToString(),
                ["pageSize"] = q.PageSize.ToString(),
                ["status"] = q.Status?.ToString(),   // 널이면 null 그대로
                ["search"] = q.Query,
                ["createdFrom"] = q.CreatedFrom?.ToString("o"),
                ["createdTo"] = q.CreatedTo?.ToString("o")
            });

            var resp = await api.GetAsync(url, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                // 화면에 원문 에러를 잠깐 보여주기(개발 중)
                return Content($"API 500\n\n{body}", "text/plain; charset=utf-8");
            }
            var page = await resp.Content.ReadFromJsonAsync<Application.Common.Models.PagedResult<UserSummaryDto>>(cancellationToken: ct)
           ?? new(Array.Empty<UserSummaryDto>(), q.Page, q.PageSize, 0);

            var vm = page.ToVm(q);
            return View(vm);
        }

        // 상세
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> Detail([FromRoute] int userId, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");
            var resp = await api.GetAsync($"/api/admin/users/{userId}", ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["err"] = "사용자를 찾을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            if (!resp.IsSuccessStatusCode)
            {
                TempData["err"] = await ReadProblemOrRaw(resp, ct);
                return RedirectToAction(nameof(Index));
            }

            var dto = await resp.Content.ReadFromJsonAsync<UserDetailDto>(cancellationToken: ct)
                      ?? throw new InvalidOperationException("응답 파싱 실패");
            return View(dto.ToVm()); // Views/AdminUsers/Detail.cshtml
        }

        // ---- 상태 변경
        [HttpPost("{userId:int}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus([FromRoute] int userId, SetStatusVm vm, CancellationToken ct)
        {
            if (userId != vm.UserId) return BadRequest();

            var api = _http.CreateClient("GameApi");
            var resp = await api.PostAsJsonAsync($"/api/admin/users/{userId}/status",
                                                 new AdminSetStatusRequest(vm.Status), ct);

            await Toast(resp, ok: "상태 변경 완료", errPrefix: "상태 변경 실패", ct);
            return RedirectToAction(nameof(Detail), new { userId });
        }

        // ---- 닉네임 변경
        [HttpPost("{userId:int}/nickname")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetNickname([FromRoute] int userId, SetNicknameVm vm, CancellationToken ct)
        {
            if (userId != vm.UserId) return BadRequest();

            var api = _http.CreateClient("GameApi");
            var resp = await api.PostAsJsonAsync($"/api/admin/users/{userId}/nickname",
                                                 new AdminSetNicknameRequest(vm.NickName), ct);

            await Toast(resp, ok: "닉네임 변경 완료", errPrefix: "닉네임 변경 실패", ct);
            return RedirectToAction(nameof(Detail), new { userId });
        }

        // ---- 비번 초기화
        [HttpPost("{userId:int}/reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromRoute] int userId, ResetPasswordVm vm, CancellationToken ct)
        {
            if (userId != vm.UserId) return BadRequest();

            var api = _http.CreateClient("GameApi");
            var resp = await api.PostAsJsonAsync($"/api/admin/users/{userId}/reset-password",
                                                 new AdminResetPasswordRequest(vm.NewPassword), ct);

            await Toast(resp, ok: "비밀번호 초기화 완료", errPrefix: "비밀번호 초기화 실패", ct);
            return RedirectToAction(nameof(Detail), new { userId });
        }

        // ---- 세션 만료(단일/전체)
        [HttpPost("{userId:int}/revoke-session")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeSession([FromRoute] int userId, RevokeSessionVm vm, CancellationToken ct)
        {
            if (userId != vm.UserId) return BadRequest();

            var api = _http.CreateClient("GameApi");
            var resp = await api.PostAsJsonAsync($"/api/admin/users/{userId}/revoke-session",
                                                 new AdminRevokeSessionRequest(vm.SessionId, vm.AllOfUser), ct);

            await Toast(resp,
                ok: vm.AllOfUser ? "모든 세션 만료됨" : "세션 만료됨",
                errPrefix: "세션 만료 실패", ct: ct);

            return RedirectToAction(nameof(Detail), new { userId });
        }

        // (옵션) 상세 탭 비동기 로드를 위한 최근 세션 페이징
        [HttpGet("{userId:int}/sessions")]
        public async Task<IActionResult> Sessions([FromRoute] int userId, int page = 1, int pageSize = 20,
                                                  bool? revoked = null, bool activeOnly = false, CancellationToken ct = default)
        {
            var api = _http.CreateClient("GameApi");

            var url = QueryHelpers.AddQueryString("/api/admin/sessions", new Dictionary<string, string?>
            {
                ["userId"] = userId.ToString(),
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString(),
                ["revoked"] = revoked?.ToString()?.ToLowerInvariant(),
                ["activeOnly"] = activeOnly.ToString().ToLowerInvariant()
            });

            var res = await api.GetFromJsonAsync<PagedResult<SessionBriefDto>>(url, ct)
                      ?? new(Array.Empty<SessionBriefDto>(), 0, page, pageSize);

            return Json(new
            {
                res.TotalCount,
                Items = res.Items.Select(s => new { s.Id, s.ExpiresAt, s.RefreshExpiresAt, s.Revoked })
            });
        }

        // ----------------- helpers -----------------

        private async Task Toast(HttpResponseMessage resp, string ok, string errPrefix, CancellationToken ct)
        {
            if (resp.IsSuccessStatusCode) { TempData["ok"] = ok; return; }
            TempData["err"] = $"{errPrefix}: {await ReadProblemOrRaw(resp, ct)}";
        }

        private static async Task<string> ReadProblemOrRaw(HttpResponseMessage resp, CancellationToken ct)
        {
            try
            {
                // 1) ValidationProblemDetails
                var raw = await resp.Content.ReadAsStringAsync(ct);
                try
                {
                    var vpd = JsonSerializer.Deserialize<ValidationProblemDetails>(raw);
                    if (vpd?.Errors?.Count > 0)
                    {
                        var sb = new StringBuilder();
                        foreach (var kv in vpd.Errors)
                            foreach (var msg in kv.Value) sb.AppendLine(msg);
                        return sb.ToString().Trim();
                    }
                }
                catch { /* ignore */ }

                // 2) ProblemDetails
                try
                {
                    var pd = JsonSerializer.Deserialize<ProblemDetails>(raw, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (!string.IsNullOrWhiteSpace(pd?.Detail)) return pd!.Detail!;
                }
                catch { /* ignore */ }

                // 3) 그냥 원문
                return $"{(int)resp.StatusCode} {resp.ReasonPhrase} - {raw}";
            }
            catch
            {
                return $"{(int)resp.StatusCode} {resp.ReasonPhrase}";
            }
        }
    }
}
