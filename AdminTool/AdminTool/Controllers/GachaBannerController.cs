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
            var client = _http.CreateClient("GameApi");

            var url = QueryHelpers.AddQueryString("/api/GachaBanner", new Dictionary<string, string?>
            {
                ["keyword"] = filter.Keyword,
                ["skip"] = filter.Skip.ToString(),
                ["take"] = filter.Take.ToString()
            });

            var res = await client.GetAsync(url, ct);
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

            var client = _http.CreateClient("GameApi");
            var req = vm.ToCreateRequest("Asia/Seoul");
            var resp = await client.PostAsJsonAsync("/api/GachaBanner", req, ct);

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
            var client = _http.CreateClient("GameApi");
            var resp = await client.GetAsync($"/api/GachaBanner/{id}", ct);
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

            var client = _http.CreateClient("GameApi");
            var req = vm.ToUpdateRequest("Asia/Seoul");
            var resp = await client.PutAsJsonAsync($"/api/GachaBanner/{id}", req, ct);

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
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/GachaBanner/{id}", ct);

            if (!resp.IsSuccessStatusCode)
                TempData["toast"] = $"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
            else
                TempData["toast"] = "배너가 삭제되었습니다.";

            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────────────────
        // 드롭다운 소스
        // ─────────────────────────────────────────────────────────────────────
        private async Task<IEnumerable<SelectListItem>> GetPoolOptionsAsync(CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");

            // WebServer 쪽 라우트 명이 다를 수 있어 Plan A/B 준비
            var poolsA = await TryGet<List<PoolRow>>(api, "/api/gacha/pools", ct);
            var poolsB = poolsA ?? await TryGet<List<PoolRow>>(api, "/api/gachapools", ct);
            var rows = poolsB ?? new List<PoolRow>();

            return rows.Select(p => new SelectListItem($"{p.Name} (#{p.PoolId})", p.PoolId.ToString())).ToList();
        }

        private async Task<IEnumerable<SelectListItem>> GetPortraitOptionsAsync(CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");
            var ports = await TryGet<List<PortraitRow>>(api, "/api/portraits", ct) ?? new();
            return ports.Select(p => new SelectListItem($"{p.Key} (#{p.PortraitId})", p.PortraitId.ToString())).ToList();
        }

        // ─────────────────────────────────────────────────────────────────────
        // 헬퍼: API 에러 파싱
        // ─────────────────────────────────────────────────────────────────────
        private static void TryParseProblemToTempData(string body)
        {
            try
            {
                var vpd = JsonSerializer.Deserialize<ValidationProblemDetails>(body);
                if (vpd?.Errors?.Count > 0)
                {
                    // TempData 알림만
                    var first = vpd.Errors.Values.FirstOrDefault()?.FirstOrDefault();
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    // (언어서버 침묵용)
                }
                else
                {
                    var pd = JsonSerializer.Deserialize<ProblemDetails>(body);
                    // 여기서는 컨트롤러 내부에서만 사용—TempData는 this 가 필요해서 생략
                }
            }
            catch { /* ignore */ }
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

        // API 응답 간단 모델(드롭다운)
        private sealed class PoolRow { public int PoolId { get; set; } public string Name { get; set; } = string.Empty; }
        private sealed class PortraitRow { public int PortraitId { get; set; } public string Key { get; set; } = string.Empty; }

        // Web API Search 응답 컨테이너(서버 컨트롤러에서 반환한 형식과 맞추기)
        private sealed record SearchResponse<T>(IReadOnlyList<T> Items, int Total, int Skip, int Take);

        // 목록에서 쓰는 DTO (서버 DTO와 동일 네임/케이스 가정)
        
    }
}
