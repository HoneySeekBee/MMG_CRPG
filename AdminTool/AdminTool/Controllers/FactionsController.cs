namespace AdminTool.Controllers
{
    using System.Net.Http.Json;
    using AdminTool.Models;
    using Application.Factions;
    using Microsoft.AspNetCore.Mvc;

    public class FactionsController : Controller
    {
        private readonly IHttpClientFactory _http;
        //private readonly ILogger<FactionsController> _logger;
        private readonly string _assetsBaseUrl;

        private readonly string _assetsPhysicalRoot;
        private readonly string _iconsSubdir;
        public FactionsController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;
            _assetsBaseUrl = cfg["PublicBaseUrl"]!.TrimEnd('/');

            _assetsPhysicalRoot = cfg["Assets:PhysicalRoot"]!
                ?? throw new InvalidOperationException("Assets:PhysicalRoot 설정이 필요합니다.");

            _iconsSubdir = cfg["Assets:IconsSubdir"] ?? "icons";
        }
        public async Task<IActionResult> Index(bool? isActive, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // (1) Factions 조회
            var url = "/api/factions";
            if (isActive != null) url += $"?isActive={isActive.Value.ToString().ToLower()}";

            var list = await client.GetFromJsonAsync<List<FactionDto>>(url, ct)
                       ?? new List<FactionDto>();

            // (2) Icons 조회
            var icons = await client.GetFromJsonAsync<List<IconVm>>("/api/icons", ct)
                       ?? new List<IconVm>();

            var iconMap = icons.ToDictionary(k => k.IconId, v => (v.Key, v.Version));

            // (3) 모델 구성
            var model = list.Select(x =>
            {
                string? iconUrl = null;
                if (x.IconId.HasValue && iconMap.TryGetValue(x.IconId.Value, out var info))
                {
                    iconUrl = $"{_assetsBaseUrl}/{_iconsSubdir}/{info.Key}.png?v={info.Version}";
                }

                return new FactionVm
                {
                    FactionId = x.FactionId,
                    Key = x.Key,
                    Label = x.Label,
                    ColorHex = x.ColorHex,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    IconId = x.IconId,
                    IconUrl = iconUrl
                };
            }).ToList();

            ViewBag.FilterIsActive = isActive;
            return View(model);
        }
        // GET: /Factions/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // 아이콘을 조회
            var apiIcons = await client.GetFromJsonAsync<List<IconVm>>("/api/icons", ct) ?? new();

            var icons = apiIcons.Select(x => new IconPickItem
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = $"{_assetsBaseUrl}/icons/{x.Key}.png?v={x.Version}"
            }).ToList();
            var vm = new FactionCreateVm
            {
                ColorHex = "#FFFFFF",
                SortOrder = 1,
                Meta = "",
                Icons = icons,
            };

            return View(vm);
        }

        // POST: /Factions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FactionCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var client = _http.CreateClient("GameApi");
            var req = new CreateFactionRequest
            {
                Key = vm.Key,
                Label = vm.Label,
                ColorHex = vm.ColorHex,
                IconId = vm.IconId,
                SortOrder = vm.SortOrder,
                IsActive = vm.IsActive,
                Meta = vm.Meta
            };

            var resp = await client.PostAsJsonAsync("/api/factions", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"생성 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
                return View(vm);
            }

            TempData["Message"] = "Faction이 생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Factions/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            var dto = await client.GetFromJsonAsync<FactionDto>($"/api/factions/{id}", ct);

            if (dto == null) return NotFound();
            // 아이콘을 조회
            var apiIcons = await client.GetFromJsonAsync<List<IconVm>>("/api/icons", ct) ?? new();

            var icons = apiIcons.Select(x => new IconPickItem
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = $"{_assetsBaseUrl}/icons/{x.Key}.png?v={x.Version}"
            }).ToList();


            var vm = new FactionEditVm
            {
                FactionId = dto.FactionId,
                Label = dto.Label,
                ColorHex = dto.ColorHex,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                IconId = dto.IconId,
                Meta = dto.Meta,

                Icons = icons,
            };
            return View(vm);
        }

        // POST: /Factions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FactionEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var client = _http.CreateClient("GameApi");
            var req = new UpdateFactionRequest
            {
                Label = vm.Label,
                ColorHex = vm.ColorHex,
                SortOrder = vm.SortOrder,
                IsActive = vm.IsActive,
                IconId = vm.IconId,
                Meta = vm.Meta
            };

            var resp = await client.PutAsJsonAsync($"/api/factions/{id}", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"수정 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
                return View(vm);
            }

            TempData["Message"] = "Faction이 수정되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Factions/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/factions/{id}", ct);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
            }
            else
            {
                TempData["Message"] = "Faction이 삭제되었습니다.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
