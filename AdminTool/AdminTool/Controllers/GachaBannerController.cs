using AdminTool.Models;
using Application.GachaBanner;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Net;
using System.Text.Json;

namespace AdminTool.Controllers
{
    [Route("GachaBanners")]
    public sealed class GachaBannersController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;

        public GachaBannersController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Index (검색/목록)
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] GachaBannerFilterVm filter, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");

            var url = QueryHelpers.AddQueryString("/api/GachaBanner", new Dictionary<string, string?>
            {
                ["keyword"] = filter.Keyword,
                ["skip"] = filter.Skip.ToString(),
                ["take"] = filter.Take.ToString()
            });

            var res = await api.GetAsync(url, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                TryParseProblemToTempData(body);
                return View(new GachaBannerIndexVm { Filter = filter });
            }

            var page = JsonSerializer.Deserialize<SearchResponse<GachaBannerDto>>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var vm = new GachaBannerIndexVm
            {
                Filter = filter,
                Items = (page?.Items ?? Array.Empty<GachaBannerDto>()).Select(GachaBannerListItemVm.FromDto).ToList(),
                Total = page?.Total ?? 0,
                Skip = filter.Skip,
                Take = filter.Take
            };
            return View(vm);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Create
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("Create")]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new GachaBannerFormVm
            {
                StartsAtLocal = DateTime.Now,
                PoolOptions = await GetPoolOptionsAsync(ct),
                PortraitOptions = await GetPortraitOptionsAsync(ct)
            };
            return View(vm);
        }
        private async Task<IEnumerable<SelectListItem>> GetPortraitOptionsAsync(CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");

            // 기본 경로(웹 API가 제공한다고 가정)
            const string primaryUrl = "/api/portraits";

            // 혹시 엔드포인트가 다르면 대비용으로 플랜 B를 더해도 됨
            // const string fallbackUrl = "/api/portrait"; // 필요 시 사용

            try
            {
                var rows = await TryGet<List<PortraitRow>>(api, primaryUrl, ct)
                           ?? new List<PortraitRow>();

                // 이름/키 기준 정렬 후 SelectListItem 변환
                return rows
                    .OrderBy(p => p.Key)
                    .Select(p => new SelectListItem($"{p.Key} (#{p.PortraitId})", p.PortraitId.ToString()))
                    .ToList();
            }
            catch
            {
                // API가 죽었거나 형식이 달라도 폼이 깨지지 않도록 빈 목록 반환
                return Enumerable.Empty<SelectListItem>();
            }
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GachaBannerFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                vm.PoolOptions = await GetPoolOptionsAsync(ct);
                vm.PortraitOptions = await GetPortraitOptionsAsync(ct);
                return View(vm);
            }

            var api = _http.CreateClient("GameApi");
            var req = vm.ToCreateRequest("Asia/Seoul");
            var resp = await api.PostAsJsonAsync("/api/GachaBanner", req, ct);

            if (!resp.IsSuccessStatusCode)
            {
                await AddModelErrorsAsync(resp, ct);
                vm.PoolOptions = await GetPoolOptionsAsync(ct);
                vm.PortraitOptions = await GetPortraitOptionsAsync(ct);
                return View(vm);
            }

            TempData["toast"] = "가차 배너가 생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Edit
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");
            var resp = await api.GetAsync($"/api/GachaBanner/{id}", ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = $"배너(id={id})를 찾을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                TryParseProblemToTempData(body);
                return RedirectToAction(nameof(Index));
            }

            var dto = JsonSerializer.Deserialize<GachaBannerDto>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto is null)
            {
                TempData["Error"] = "API 응답을 해석할 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            var vm = GachaBannerFormVm.FromDto(dto, "Asia/Seoul");
            vm.PoolOptions = await GetPoolOptionsAsync(ct);
            vm.PortraitOptions = await GetPortraitOptionsAsync(ct);
            return View(vm);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GachaBannerFormVm vm, CancellationToken ct)
        {
            if (vm.Id != id) return BadRequest("Id mismatch");
            if (!ModelState.IsValid)
            {
                vm.PoolOptions = await GetPoolOptionsAsync(ct);
                vm.PortraitOptions = await GetPortraitOptionsAsync(ct);
                return View(vm);
            }

            var api = _http.CreateClient("GameApi");
            var req = vm.ToUpdateRequest("Asia/Seoul");
            var resp = await api.PutAsJsonAsync($"/api/GachaBanner/{id}", req, ct);

            if (!resp.IsSuccessStatusCode)
            {
                await AddModelErrorsAsync(resp, ct);
                vm.PoolOptions = await GetPoolOptionsAsync(ct);
                vm.PortraitOptions = await GetPortraitOptionsAsync(ct);
                return View(vm);
            }

            TempData["toast"] = "배너가 수정되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Delete
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost("{id:int}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");
            var resp = await api.DeleteAsync($"/api/GachaBanner/{id}", ct);

            if (!resp.IsSuccessStatusCode)
                TempData["toast"] = $"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
            else
                TempData["toast"] = "배너가 삭제되었습니다.";

            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────────────
        // 풀 드롭다운: 초기 로딩(list) + AJAX 검색(search)
        // ─────────────────────────────────────────────────────────────────────
        private async Task<IEnumerable<SelectListItem>> GetPoolOptionsAsync(CancellationToken ct, int take = 200)
        {
            var api = _http.CreateClient("GameApi");

            // Web API: /api/gacha/pools/list (드롭다운용 경량 목록)
            var listUrl = QueryHelpers.AddQueryString("/api/gacha/pools/list", new Dictionary<string, string?>
            {
                ["take"] = take.ToString()
            });

            var rows = await TryGet<List<PoolRow>>(api, listUrl, ct) ?? new();
            return rows
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem($"{p.Name} (#{p.PoolId})", p.PoolId.ToString()))
                .ToList();
        }

        [HttpGet("PoolOptions")]
        public async Task<IActionResult> PoolOptions([FromQuery] string? keyword, [FromQuery] int take = 50, CancellationToken ct = default)
        {
            var api = _http.CreateClient("GameApi");

            // Web API: /api/gacha/pools?keyword&skip&take (검색)
            var url = QueryHelpers.AddQueryString("/api/gacha/pools", new Dictionary<string, string?>
            {
                ["keyword"] = string.IsNullOrWhiteSpace(keyword) ? null : keyword,
                ["skip"] = "0",
                ["take"] = take.ToString()
            });

            var body = await api.GetStringAsync(url, ct);
            var page = JsonSerializer.Deserialize<SearchResponse<GachaPoolLite>>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var items = (page?.Items ?? Array.Empty<GachaPoolLite>())
                .Select(p => new { id = p.PoolId, text = $"{p.Name} (#{p.PoolId})" })
                .ToList();

            return Json(items);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 에러 파싱 & 공용 헬퍼
        // ─────────────────────────────────────────────────────────────────────
        private static void TryParseProblemToTempData(string body)
        {
            try
            {
                var vpd = JsonSerializer.Deserialize<ValidationProblemDetails>(body);
                if (vpd?.Errors?.Count > 0)
                {
                    var first = vpd.Errors.Values.FirstOrDefault()?.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(first))
                        // TempData를 쓰려면 this가 필요하므로 호출자에서 처리 권장
                        ; // 알림만 필요하면 여기에 메시지 세팅 가능
                    return;
                }

                var pd = JsonSerializer.Deserialize<ProblemDetails>(body);
                // TempData 사용 필요시 호출자에서 처리
            }
            catch { /* ignore */ }
        }
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Detail(int id, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");
            var resp = await api.GetAsync($"/api/GachaBanner/{id}", ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = $"배너(id={id})를 찾을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                // 필요 시 상세 에러 파싱
                return RedirectToAction(nameof(Index));
            }

            var dto = JsonSerializer.Deserialize<GachaBannerDto>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto is null)
            {
                TempData["Error"] = "API 응답을 해석할 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            var vm = GachaBannerFormVm.FromDto(dto, "Asia/Seoul");
            // 드롭다운 소스는 표시용으로만 필요 (선택 비활성)
            vm.PoolOptions = await GetPoolOptionsAsync(ct);
            vm.PortraitOptions = await GetPortraitOptionsAsync(ct);
            return View(vm); // Views/GachaBanners/Details.cshtml
        }
        private async Task AddModelErrorsAsync(HttpResponseMessage resp, CancellationToken ct)
        {
            var raw = await resp.Content.ReadAsStringAsync(ct);
            try
            {
                var vpd = JsonSerializer.Deserialize<ValidationProblemDetails>(raw);
                if (vpd?.Errors?.Count > 0)
                {
                    foreach (var kv in vpd.Errors)
                        foreach (var msg in kv.Value)
                            ModelState.AddModelError(kv.Key ?? string.Empty, msg);
                    return;
                }
                var pd = JsonSerializer.Deserialize<ProblemDetails>(raw);
                if (pd?.Detail is not null) ModelState.AddModelError(string.Empty, pd.Detail);
                else ModelState.AddModelError(string.Empty, raw);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, raw);
            }
        }

        private static async Task<T?> TryGet<T>(HttpClient c, string url, CancellationToken ct)
        {
            try
            {
                var resp = await c.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode) return default;
                var body = await resp.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch { return default; }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 내부 DTO (드롭다운/검색용 경량)
        // ─────────────────────────────────────────────────────────────────────
        private sealed class PoolRow
        {
            public int PoolId { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private sealed class PortraitRow
        {
            public int PortraitId { get; set; }
            public string Key { get; set; } = string.Empty;
        }

        private sealed class GachaPoolLite
        {
            public int PoolId { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private sealed record SearchResponse<T>(IReadOnlyList<T> Items, int Total, int Skip, int Take);
    }
    // 목록에서 쓰는 DTO (서버 DTO와 동일 네임/케이스 가정)

}
