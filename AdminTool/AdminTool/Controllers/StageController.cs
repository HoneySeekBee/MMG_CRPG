using AdminTool.Models; 
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Application.Common.Models;
using Application.Items;
using Application.Contents.Stages;

namespace AdminTool.Controllers
{
    [Route("stages")]
    public sealed class StagesController : Controller
    {
        private readonly IHttpClientFactory _http;
        private static readonly JsonSerializerOptions _json
            = new(JsonSerializerDefaults.Web);

        public StagesController(IHttpClientFactory http)
        {
            _http = http;
        }

        private HttpClient CreateClient() => _http.CreateClient("GameApi");
     
        // ─────────────────────────────────────────────
        // Index
        // ─────────────────────────────────────────────
        [HttpGet("")]
        public async Task<IActionResult> Index(
            int page = 1, int pageSize = 20, int? chapter = null, bool? isActive = null, string? search = null,
            CancellationToken ct = default)
        {
            var vm = new StageIndexVm
            {
                Filter = new StageListFilterVm
                {
                    Page = page,
                    PageSize = pageSize,
                    Chapter = chapter,
                    IsActive = isActive,
                    Search = search,
                    Chapters = MakeChapters(1, 20),
                    ActiveFlags = new[]
                    {
                        new SelectListItem("전체", ""),
                        new SelectListItem("활성", "true"),
                        new SelectListItem("비활성", "false"),
                    }
                }
            };

            var client = CreateClient();
            var url = $"/api/stages?page={vm.Filter.Page}&pageSize={vm.Filter.PageSize}"
                    + (chapter.HasValue ? $"&chapter={chapter}" : "")
                    + (isActive.HasValue ? $"&isActive={isActive.Value.ToString().ToLower()}" : "")
                    + (!string.IsNullOrWhiteSpace(search) ? $"&search={Uri.EscapeDataString(search)}" : "");

            var result = await client.GetFromJsonAsync<PagedResult<StageSummaryDto>>(url, _json, ct);
            if (result is null) return View(vm);

            vm.Items = result.Items.Select(StageVmMapper.FromDto).ToList();
            vm.TotalCount = (int)result.TotalCount;
            return View(vm);
        }

        // ─────────────────────────────────────────────
        // Create
        // ─────────────────────────────────────────────
        [HttpGet("create")]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = await BuildFormVmAsync(null, ct);
            return View("Form", vm);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] StageFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                vm = await BuildFormVmAsync(vm, ct);
                return View("Form", vm);
            }

            var req = vm.ToCreateRequest();
            var client = CreateClient();
            var resp = await client.PostAsJsonAsync("/api/stages", req, _json, ct);

            if (!resp.IsSuccessStatusCode)
            {
                await BindApiProblemAsync(resp);
                vm = await BuildFormVmAsync(vm, ct);
                return View("Form", vm);
            }

            TempData["Toast"] = "스테이지가 생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────
        // Edit
        // ─────────────────────────────────────────────
        [HttpGet("{id:int}/edit")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var client = CreateClient();
            var dto = await client.GetFromJsonAsync<StageDetailDto>($"/api/stages/{id}", _json, ct);
            if (dto is null) return NotFound();

            var vm = await BuildFormVmAsync(null, ct, dto);
            return View("Form", vm);
        }

        [HttpPost("{id:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] StageFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                vm = await BuildFormVmAsync(vm, ct);
                return View("Form", vm);
            }

            var req = vm.ToUpdateRequest(id);
            var client = CreateClient();
            var resp = await client.PutAsJsonAsync($"/api/stages/{id}", req, _json, ct);

            if (!resp.IsSuccessStatusCode)
            {
                await BindApiProblemAsync(resp);
                vm = await BuildFormVmAsync(vm, ct);
                return View("Form", vm);
            }

            TempData["Toast"] = "스테이지가 수정되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────
        // Delete
        // ─────────────────────────────────────────────
        [HttpPost("{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = CreateClient();
            var resp = await client.DeleteAsync($"/api/stages/{id}", ct);
            if (!resp.IsSuccessStatusCode)
            {
                await BindApiProblemAsync(resp);
            }
            else
            {
                TempData["Toast"] = "삭제되었습니다.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────
        
        private async Task<List<T>> GetAllPagedAsync<T>(HttpClient api, string baseUrl, int pageSize, CancellationToken ct)
        {
            var all = new List<T>();
            var page = 1;

            while (true)
            {
                var url = $"{baseUrl}?page={page}&pageSize={pageSize}";
                using var resp = await api.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    // 400 등은 중단하고 현재까지 수집된 것만 반환 (폼은 떠야 하니까)
                    break;
                }

                var pr = await resp.Content.ReadFromJsonAsync<Application.Common.Models.PagedResult<T>>(_json, ct);
                if (pr is null || pr.Items.Count == 0) break;

                all.AddRange(pr.Items);

                if (all.Count >= pr.TotalCount) break;
                page++;
            }
            return all;
        }
        private async Task<StageFormVm> BuildFormVmAsync(StageFormVm? incoming, CancellationToken ct, StageDetailDto? detail = null)
        {
            // 드롭다운 데이터 로딩
            var client = CreateClient();

            // 적 캐릭터 목록(간단 버전: 전부)
            var enemies = await client.GetFromJsonAsync<PagedResult<Application.Character.CharacterSummaryDto>>(
                "/api/characters?page=1&pageSize=1000", _json, ct);
            var enemyOptions = (enemies?.Items ?? Array.Empty<Application.Character.CharacterSummaryDto>())
                .Select(c => new SelectListItem($"{c.Name} (#{c.Id})", c.Id.ToString()))
                .ToList();

            // 아이템 목록
            var itemDtos = await GetAllPagedAsync<Application.Items.ItemDto>(client, "/api/items", 200, ct);
            var itemOptions = itemDtos
                .Select(i => new SelectListItem($"{i.Name} (#{i.Id})", i.Id.ToString()))
                .ToList();

            // 스테이지 목록(요구조건 선택용)
            var stages = await client.GetFromJsonAsync<PagedResult<StageSummaryDto>>(
                "/api/stages?page=1&pageSize=1000", _json, ct);
            var stageOptions = (stages?.Items ?? Array.Empty<StageSummaryDto>())
                .Select(s => new SelectListItem($"Ch{s.Chapter}-{s.StageNum} {s.Name}", s.Id.ToString()))
                .ToList();

            // 챕터 드롭다운(1~20 기본)
            var chapterOptions = MakeChapters(1, 20);

            if (detail is not null)
            {
                return StageVmMapper.FromDetailDto(detail, enemyOptions, itemOptions, stageOptions, chapterOptions);
            }

            var vm = incoming ?? new StageFormVm();
            vm.EnemyOptions = enemyOptions;
            vm.ItemOptions = itemOptions;
            vm.StageOptions = stageOptions;
            vm.ChapterOptions = chapterOptions;

            // 초기 행 없으면 1웨이브/한 줄 생성
            if (vm.Waves.Count == 0)
                vm.Waves.Add(new WaveVm
                {
                    Index = 1,
                    Enemies = new List<EnemyRowVm> { new EnemyRowVm { Slot = 1, Enemies = enemyOptions } }
                });

            return vm;
        }

        private static IEnumerable<SelectListItem> MakeChapters(int from, int toInclusive)
            => Enumerable.Range(from, toInclusive - from + 1)
                         .Select(i => new SelectListItem($"Chapter {i}", i.ToString()));

        private async Task BindApiProblemAsync(HttpResponseMessage resp)
        {
            try
            {
                var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>(_json);
                if (problem is not null)
                    ModelState.AddModelError(string.Empty, problem.Detail ?? problem.Title ?? $"API Error {resp.StatusCode}");
                else
                    ModelState.AddModelError(string.Empty, $"API Error {resp.StatusCode}");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, $"API Error {resp.StatusCode}");
            }
        }
    }
}
