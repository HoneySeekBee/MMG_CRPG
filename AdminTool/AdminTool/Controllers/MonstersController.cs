using AdminTool.Models;
using Application.Elements;
using Application.Monsters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdminTool.Controllers
{
    public class MonstersController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;

        public MonstersController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;
        }

        // GET: /Monster
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            var vm = new MonsterIndexVm();

            var resp = await client.GetAsync("/api/monster", ct);
            if (!resp.IsSuccessStatusCode)
            {
                // API가 죽었을 때도 화면은 보여주자
                ViewData["ApiError"] = $"API /api/monster 호출 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
                return View(vm);
            }

            // 여기까지 왔으면 OK
            var apiMonsters = await resp.Content.ReadFromJsonAsync<List<MonsterDtoStub>>(cancellationToken: ct)
                              ?? new List<MonsterDtoStub>();
            var ports = await client.GetFromJsonAsync<List<PortraitVm>>("/api/portraits", ct) ?? new();
            var baseUrl = _cfg["PublicBaseUrl"]!.TrimEnd('/');
            var subdir = _cfg["Assets:PortraitsSubdir"] ?? "portraits";
            vm.Monsters = apiMonsters.Select(m =>
            {
                string? portraitUrl = null;
                if (m.PortraitId is not null)
                {
                    var p = ports.FirstOrDefault(x => x.PortraitId == m.PortraitId);
                    if (p != null)
                    {
                        portraitUrl = $"{baseUrl}/{subdir}/{p.Key}.png?v={p.Version}";
                    }
                }

                return new MonsterListItemVm
                {
                    Id = m.Id,
                    Name = m.Name,
                    ModelKey = m.ModelKey,
                    ElementId = m.ElementId,
                    PortraitId = m.PortraitId,
                    StatCount = m.Stats?.Count ?? 0,
                    PortraitUrl = portraitUrl 
                };
            }).ToList();
            return View(vm);
        }

        // GET: /Monster/Create
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new MonsterEditVm();
            await FillPortraitsAsync(vm, ct);
            await FillElementsAsync(vm, ct);
            return View(vm);
        }

        // POST: /Monster/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonsterEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await FillPortraitsAsync(vm, ct);
                await FillElementsAsync(vm, ct);
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");

            var req = new
            {
                name = vm.Name,
                modelKey = vm.ModelKey,
                elementId = vm.ElementId,
                portraitId = vm.PortraitId,
                stats = Array.Empty<object>() // 처음엔 비워둠
            };

            var resp = await client.PostAsJsonAsync("/api/monster", req, ct);
            resp.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Monster/Edit/5
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var apiMonster = await client.GetFromJsonAsync<MonsterDtoStub>($"/api/monster/{id}", ct);

            if (apiMonster is null)
                return NotFound();

            var vm = new MonsterEditVm
            {
                Id = id,
                Name = apiMonster.Name,
                ModelKey = apiMonster.ModelKey,
                ElementId = apiMonster.ElementId,
                PortraitId = apiMonster.PortraitId
            };

            await FillPortraitsAsync(vm, ct);
            await FillElementsAsync(vm, ct);
            return View(vm);
        }
        // POST: /Monster/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MonsterEditVm vm, CancellationToken ct)
        {
            if (id != vm.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await FillPortraitsAsync(vm, ct);
                await FillElementsAsync(vm, ct);
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");

            var req = new
            {
                id = vm.Id,
                name = vm.Name,
                modelKey = vm.ModelKey,
                elementId = vm.ElementId,
                portraitId = vm.PortraitId
            };

            var resp = await client.PutAsJsonAsync($"/api/monster/{id}", req, ct);
            resp.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Monster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/monster/{id}", ct);
            resp.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }
        private async Task FillPortraitsAsync(MonsterEditVm vm, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var apiPorts = await client.GetFromJsonAsync<List<PortraitVm>>("/api/portraits", ct) ?? new();

            var baseUrl = _cfg["PublicBaseUrl"]!.TrimEnd('/');
            var subdir = _cfg["Assets:PortraitsSubdir"] ?? "portraits";

            vm.PortraitChoices = apiPorts
                .Select(p => new PortraitPickItem
                {
                    PortraitId = p.PortraitId,
                    Key = p.Key,
                    Version = p.Version,
                    Url = $"{baseUrl}/{subdir}/{p.Key}.png?v={p.Version}"
                })
                .ToList();

            if (vm.PortraitId.HasValue)
            {
                vm.SelectedPortraitUrl = vm.PortraitChoices
                    .FirstOrDefault(x => x.PortraitId == vm.PortraitId.Value)
                    ?.Url;
            }
        }
        private async Task FillElementsAsync(MonsterEditVm vm, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // 라우트가 단수/복수일 수 있으니 둘 다 시도
            var elems =
                await TryGet<List<ElementDto>>(client, "/api/element", ct)
                ?? await TryGet<List<ElementDto>>(client, "/api/elements", ct)
                ?? new List<ElementDto>();

            vm.Elements = elems
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.ElementId)
                .Select(e => new SelectListItem(
                    text: e.Label,
                    value: e.ElementId.ToString(),
                    selected: e.ElementId == vm.ElementId
                ))
                .ToList();
        }
        private static async Task<T?> TryGet<T>(HttpClient client, string url, CancellationToken ct)
        {
            try
            {
                var resp = await client.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode)
                    return default;

                return await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
            }
            catch
            {
                return default;
            }
        }

        // 이 컨트롤러 안에서만 쓸 간단한 DTO
        private sealed class ElementDto
        {
            public int ElementId { get; set; }
            public string Label { get; set; } = string.Empty;
            public int SortOrder { get; set; }
        }
    }
} 