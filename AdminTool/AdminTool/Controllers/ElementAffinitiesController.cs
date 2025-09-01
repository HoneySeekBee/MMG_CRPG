using AdminTool.Models;
using Application.ElementAffinities;
using Application.Elements;
using Microsoft.AspNetCore.Mvc;

namespace AdminTool.Controllers
{
    public class ElementAffinitiesController : Controller
    {
        private readonly IHttpClientFactory _http;
        public ElementAffinitiesController(IHttpClientFactory http) => _http = http;

        private async Task<List<ElementOptionVm>> LoadElementOptionsAsync(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            // 요소 전체 (필요하면 페이지 파라미터 조정)
            var elems = await client.GetFromJsonAsync<List<Application.Elements.ElementDto>>("/api/element?page=1&pageSize=500", ct)
                        ?? new List<Application.Elements.ElementDto>();
            return elems
                .OrderBy(e => e.Label).ThenBy(e => e.Key)
                .Select(e => new ElementOptionVm { ElementId = e.ElementId, Key = e.Key, Label = e.Label })
                .ToList();
        }

        public async Task<IActionResult> Index(int? attacker, int? defender, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            string url = "/api/elementaffinity";
            var qs = new List<string>();
            if (attacker is not null) qs.Add($"attacker={attacker}");
            if (defender is not null) qs.Add($"defender={defender}");
            if (qs.Count > 0) url += "?" + string.Join("&", qs);

            // 목록
            var list = await client.GetFromJsonAsync<List<Application.ElementAffinities.ElementAffinityDto>>(url, ct)
                       ?? new List<Application.ElementAffinities.ElementAffinityDto>();

            // 라벨 표시를 위해 요소 맵 구성
            var options = await LoadElementOptionsAsync(ct);
            var map = options.ToDictionary(x => x.ElementId, x => x.ToString());

            var model = list.Select(x => new ElementAffinityVm
            {
                AttackerElementId = x.AttackerElementId,
                DefenderElementId = x.DefenderElementId,
                Multiplier = x.Multiplier,
                AttackerElementLabel = map.TryGetValue(x.AttackerElementId, out var al) ? al : $"#{x.AttackerElementId}",
                DefenderElementLabel = map.TryGetValue(x.DefenderElementId, out var dl) ? dl : $"#{x.DefenderElementId}",
            }).ToList();

            ViewBag.FilterAttacker = attacker;
            ViewBag.FilterDefender = defender;
            ViewBag.ElementOptions = options;

            return View(model);
        }
        // [2] Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new ElementAffinityCreateVm
            {
                Elements = await LoadElementOptionsAsync(ct)
            };
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ElementAffinityCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                vm.Elements = await LoadElementOptionsAsync(ct);
                return View(vm);
            }
            if (vm.AttackerElementId == vm.DefenderElementId)
            {
                ModelState.AddModelError("", "공격/방어 속성이 같습니다.");
                vm.Elements = await LoadElementOptionsAsync(ct);
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");
            var req = new Models.CreateElementAffinityRequest
            {
                AttackerElementId = vm.AttackerElementId,
                DefenderElementId = vm.DefenderElementId,
                Multiplier = vm.Multiplier
            };
            var resp = await client.PostAsJsonAsync("/api/elementaffinity", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"생성 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
                vm.Elements = await LoadElementOptionsAsync(ct);
                return View(vm);
            }

            TempData["Message"] = "상성이 생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }
        // [3] Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int attacker, int defender, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var dto = await client.GetFromJsonAsync<Application.ElementAffinities.ElementAffinityDto>($"/api/elementaffinity/{attacker}/{defender}", ct);
            if (dto == null) { TempData["Error"] = "대상을 찾을 수 없습니다."; return RedirectToAction(nameof(Index)); }

            var options = await LoadElementOptionsAsync(ct);
            var map = options.ToDictionary(x => x.ElementId, x => x.ToString());

            var vm = new ElementAffinityEditVm
            {
                AttackerElementId = attacker,
                DefenderElementId = defender,
                AttackerElementLabel = map.TryGetValue(attacker, out var al) ? al : $"#{attacker}",
                DefenderElementLabel = map.TryGetValue(defender, out var dl) ? dl : $"#{defender}",
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ElementAffinityEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var client = _http.CreateClient("GameApi");
            var req = new Models.UpdateElementAffinityRequest { Multiplier = vm.Multiplier };
            var resp = await client.PutAsJsonAsync($"/api/elementaffinity/{vm.AttackerElementId}/{vm.DefenderElementId}", req, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"수정 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
                return View(vm);
            }

            TempData["Message"] = "수정되었습니다.";
            return RedirectToAction(nameof(Index));
        }// [4] Delete (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int attacker, int defender, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/elementaffinity/{attacker}/{defender}", ct);

            if (resp.IsSuccessStatusCode)
                TempData["Message"] = "삭제되었습니다.";
            else
                TempData["Error"] = $"삭제 실패: {resp.StatusCode}";

            return RedirectToAction(nameof(Index));
        }
    }
}
