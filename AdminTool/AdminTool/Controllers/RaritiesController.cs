using AdminTool.Models;
using Application.Rarities;
using Microsoft.AspNetCore.Mvc;

namespace AdminTool.Controllers
{
    public class RaritiesController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<RaritiesController> _logger;

        public RaritiesController(IHttpClientFactory http, ILogger<RaritiesController> logger)
        {
            _http = http;
            _logger = logger;
        }

        // GET: /Rarities
        public async Task<IActionResult> Index(bool? isActive, int? stars, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // /api/rarities?isActive=true&stars=5
            var qs = new List<string>();
            if (isActive != null) qs.Add($"isActive={isActive.Value.ToString().ToLower()}");
            if (stars != null) qs.Add($"stars={stars.Value}");
            var url = "/api/rarities" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");

            var list = await client.GetFromJsonAsync<List<RarityDto>>(url, ct)
                       ?? new List<RarityDto>();

            var model = list.Select(x => new RarityVm
            {
                RarityId = x.RarityId,
                Stars = x.Stars,                 // Application.Rarities.RarityDto의 Stars
                Key = x.Key,
                Label = x.Label,
                ColorHex = x.ColorHex,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                Meta = x.Meta
            }).ToList();

            ViewBag.FilterIsActive = isActive;
            ViewBag.FilterStars = stars;

            return View(model);
        }

        // GET: /Rarities/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new RarityCreateVm());
        }

        // POST: /Rarities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RarityCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var client = _http.CreateClient("GameApi");
            var req = new CreateRarityRequest
            {
                Stars = (short)vm.Stars,        // DTO가 short이면 캐스팅
                Key = vm.Key,
                Label = vm.Label,
                ColorHex = vm.ColorHex,
                SortOrder = vm.SortOrder,
                IsActive = vm.IsActive,
                Meta = vm.Meta
            };

            var resp = await client.PostAsJsonAsync("/api/rarities", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"생성 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
                return View(vm);
            }

            TempData["Message"] = "Rarity가 생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Rarities/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var dto = await client.GetFromJsonAsync<RarityDto>($"/api/rarities/{id}", ct);
            if (dto == null) return NotFound();

            var vm = new RarityEditVm
            {
                RarityId = dto.RarityId,
                Stars = dto.Stars,
                Label = dto.Label,
                ColorHex = dto.ColorHex,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                Meta = dto.Meta
            };
            return View(vm);
        }

        // POST: /Rarities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RarityEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var client = _http.CreateClient("GameApi");
            var req = new UpdateRarityRequest
            {
                Stars = (short)vm.Stars,
                Label = vm.Label,
                ColorHex = vm.ColorHex,
                SortOrder = vm.SortOrder,
                IsActive = vm.IsActive,
                Meta = vm.Meta
            };

            var resp = await client.PutAsJsonAsync($"/api/rarities/{id}", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"수정 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
                return View(vm);
            }

            TempData["Message"] = "Rarity가 수정되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Rarities/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/rarities/{id}", ct);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
            }
            else
            {
                TempData["Message"] = "Rarity가 삭제되었습니다.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
