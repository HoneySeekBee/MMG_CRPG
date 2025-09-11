using AdminTool.Models;
using Application.GachaPool;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text.Json;

namespace AdminTool.Controllers
{
    [Route("GachaPools")]
    public sealed class GachaPoolsController : Controller
    {
        private readonly IHttpClientFactory _http;

        public GachaPoolsController(IHttpClientFactory http) => _http = http;

        // ──────────────────────────────────────────────────────────────────
        // 목록/검색
        // ──────────────────────────────────────────────────────────────────
        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] GachaPoolFilterVm filter, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");
            var url = QueryHelpers.AddQueryString("/api/gacha/pools", new Dictionary<string, string?>
            {
                ["keyword"] = filter.Keyword,
                ["skip"] = filter.Skip.ToString(),
                ["take"] = filter.Take.ToString()
            });

            var resp = await api.GetAsync(url, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                TryParseProblemToTempData(body);
                return View(new GachaPoolIndexVm { Filter = filter });
            }

            var page = JsonSerializer.Deserialize<SearchResponse<GachaPoolDto>>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var vm = new GachaPoolIndexVm
            {
                Filter = filter,
                Items = (page?.Items ?? Array.Empty<GachaPoolDto>()).Select(GachaPoolListItemVm.FromDto).ToList(),
                Total = page?.Total ?? 0,
                Skip = filter.Skip,
                Take = filter.Take
            };
            return View(vm);
        }

        // ──────────────────────────────────────────────────────────────────
        // Create
        // ──────────────────────────────────────────────────────────────────
        [HttpGet("Create")]
        public async Task<IActionResult> Create([FromQuery] CharacterPickFilter pick, CancellationToken ct)
        {
            var vm = new GachaPoolFormVm
            {
                ScheduleStartLocal = DateTime.Now,
                CharacterOptions = await GetCharacterOptionsAsync(pick, ct)
            };
            return View(vm);
        }
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GachaPoolFormVm vm, [FromQuery] CharacterPickFilter pick, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                vm.CharacterOptions = await GetCharacterOptionsAsync(pick, ct);
                return View(vm);
            }
            Console.WriteLine("GachaPool Create 1");
            


            var api = _http.CreateClient("GameApi");

            // 1) 풀 생성
            var createReq = vm.ToCreateRequest("Asia/Seoul");

            var resp = await api.PostAsJsonAsync("/api/gacha/pools", createReq, ct);

            if (!resp.IsSuccessStatusCode)
            {
                await AddModelErrorsAsync(resp, ct);
                vm.CharacterOptions = await GetCharacterOptionsAsync(pick, ct);
                return View(vm);
            }

            Console.WriteLine("GachaPool Create ");
            var id = await resp.Content.ReadFromJsonAsync<IdOnly>(cancellationToken: ct);
            if (id is null)
            {
                ModelState.AddModelError(string.Empty, "API 응답을 해석할 수 없습니다.");
                vm.CharacterOptions = await GetCharacterOptionsAsync(pick, ct);
                return View(vm);
            }

            Console.WriteLine("GachaPool Create 3");
            // 2) 엔트리 업서트
            if (vm.Entries.Count > 0)
            {
                var upsert = vm.ToUpsertEntriesRequest() with { PoolId = id.Id };
                var resp2 = await api.PutAsJsonAsync($"/api/gacha/pools/{id.Id}/entries", upsert, ct);
                if (!resp2.IsSuccessStatusCode)
                {
                    await AddModelErrorsAsync(resp2, ct);
                    vm.CharacterOptions = await GetCharacterOptionsAsync(pick, ct);
                    return View(vm);
                }
            }
            Console.WriteLine("GachaPool Create 4");

            TempData["toast"] = "가챠풀이 생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }
        // ──────────────────────────────────────────────────────────────────
        // Edit
        // ──────────────────────────────────────────────────────────────────
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromQuery] CharacterPickFilter pick, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");
            var resp = await api.GetAsync($"/api/gacha/pools/{id}", ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                TryParseProblemToTempData(body);
                return RedirectToAction(nameof(Index));
            }

            var dto = JsonSerializer.Deserialize<GachaPoolDetailDto>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto is null)
            {
                TempData["Error"] = "API 응답을 해석할 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            var vm = GachaPoolFormVm.FromDetailDto(dto, "Asia/Seoul");
            vm.CharacterOptions = await GetCharacterOptionsAsync(pick, ct);
            return View(vm);
        }
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GachaPoolFormVm vm, [FromQuery] CharacterPickFilter pick, CancellationToken ct)
        {
            if (vm.PoolId != id) return BadRequest("Id mismatch");
            if (!ModelState.IsValid)
            {
                vm.CharacterOptions = await GetCharacterOptionsAsync(pick, ct);
                return View(vm);
            }

            var api = _http.CreateClient("GameApi");

            var updateReq = vm.ToUpdateRequest("Asia/Seoul");
            var resp = await api.PutAsJsonAsync($"/api/gacha/pools/{id}", updateReq, ct);
            if (!resp.IsSuccessStatusCode)
            {
                await AddModelErrorsAsync(resp, ct);
                vm.CharacterOptions = await GetCharacterOptionsAsync(pick, ct);
                return View(vm);
            }

            var upsert = vm.ToUpsertEntriesRequest() with { PoolId = id };
            var resp2 = await api.PutAsJsonAsync($"/api/gacha/pools/{id}/entries", upsert, ct);
            if (!resp2.IsSuccessStatusCode)
            {
                await AddModelErrorsAsync(resp2, ct);
                vm.CharacterOptions = await GetCharacterOptionsAsync(pick, ct);
                return View(vm);
            }

            TempData["toast"] = "가챠풀이 저장되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────────
        // Delete
        // ──────────────────────────────────────────────────────────────────
        [HttpPost("{id:int}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");
            var resp = await api.DeleteAsync($"/api/gacha/pools/{id}", ct);

            TempData["toast"] = resp.IsSuccessStatusCode
                ? "가챠풀이 삭제되었습니다."
                : $"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────────
        // 헬퍼
        // ──────────────────────────────────────────────────────────────────
        private async Task<IEnumerable<SelectListItem>> GetCharacterOptionsAsync(CharacterPickFilter filter, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");

            var url = QueryHelpers.AddQueryString("/api/characters", new Dictionary<string, string?>
            {
                ["page"] = "1",
                ["pageSize"] = filter.PageSize.ToString(),
                ["elementId"] = filter.ElementId?.ToString(),
                ["rarityId"] = filter.RarityId?.ToString(),
                ["search"] = filter.Search
            });

            var resp = await api.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return Enumerable.Empty<SelectListItem>();

            var body = await resp.Content.ReadAsStringAsync(ct);
            var paged = JsonSerializer.Deserialize<PagedResult<CharacterSummaryDto>>(body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new PagedResult<CharacterSummaryDto>();

            return paged.Items
                        .OrderBy(c => c.Name)
                        .Select(c => new SelectListItem($"{c.Name} (#{c.Id})", c.Id.ToString()))
                        .ToList();
        }
        private void TryParseProblemToTempData(string body)
        {
            try
            {
                var vpd = System.Text.Json.JsonSerializer.Deserialize<ValidationProblemDetails>(body);
                if (vpd?.Errors?.Count > 0)
                {
                    var first = vpd.Errors.Values.FirstOrDefault()?.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(first))
                        TempData["Error"] = first;
                    return;
                }

                var pd = System.Text.Json.JsonSerializer.Deserialize<ProblemDetails>(body);
                if (!string.IsNullOrWhiteSpace(pd?.Detail))
                    TempData["Error"] = pd.Detail;
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
                ModelState.AddModelError(string.Empty, pd?.Detail ?? raw);
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
        public sealed class CharacterPickFilter
        {
            public string? Search { get; set; }
            public int? ElementId { get; set; }
            public int? RarityId { get; set; }
            public int PageSize { get; set; } = 500; // 필요시 조절
        }
        private sealed class CharacterSummaryDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
        private sealed class PagedResult<T>
        {
            public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }
        // API 응답 경량 타입들
        private sealed record IdOnly(int Id);
        private sealed record SearchResponse<T>(IReadOnlyList<T> Items, int Total, int Skip, int Take);
        private sealed class CharacterPickRow { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
    }
}
