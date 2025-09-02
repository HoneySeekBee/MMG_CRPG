using AdminTool.Models;
using Application.Elements;
using Application.Icons;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace AdminTool.Controllers
{
    public class ElementsController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly string _assetsBaseUrl;

        private readonly string _assetsPhysicalRoot;
        private readonly string _iconsSubdir;
        public ElementsController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;
            _assetsBaseUrl = cfg["PublicBaseUrl"]!.TrimEnd('/');

            _assetsPhysicalRoot = cfg["Assets:PhysicalRoot"]!
                ?? throw new InvalidOperationException("Assets:PhysicalRoot 설정이 필요합니다.");

            _iconsSubdir = cfg["Assets:IconsSubdir"] ?? "icons";
        }

        // [1] 읽기 
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // (1) Element 조회 
            var Elements = await client.GetFromJsonAsync<List<Application.Elements.ElementDto>>("/api/element", ct)
                ?? new List<Application.Elements.ElementDto>();

            // (2) Icon 조회
            var icons = await client.GetFromJsonAsync<List<IconVm>>("/api/icons", ct)
                ?? new List<IconVm>();

            var iconMap = icons.ToDictionary(k => k.IconId, v => (v.Key, v.Version));

            var model = Elements.Select(x =>
            {
                string? iconUrl = null;
                if(x.IconId.HasValue && iconMap.TryGetValue(x.IconId.Value, out var info))
                {
                    iconUrl = $"{_assetsBaseUrl}/icons/{info.Key}.png?v={info.Version}";
                }
                return new ElementVm
                {
                    ElementId = x.ElementId,
                    Key = x.Key,
                    Label = x.Label,
                    IconId = x.IconId,
                    IconUrl = iconUrl,
                    ColorHex = x.ColorHex,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    Meta = x.Meta,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                };
            }).ToList();

            return View(model);
        }
        private async Task<List<IconPickItem>> LoadIconsAsync(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var apiIcons = await client.GetFromJsonAsync<List<IconVm>>("/api/icons", ct) ?? new();

            return apiIcons.Select(x => new IconPickItem
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = $"{_assetsBaseUrl}/icons/{x.Key}.png?v={x.Version}"
            }).ToList();
        }
        // [2] 쓰기 Create
        // GET
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
            var vm = new ElementCreateVm
            {
                ColorHex = "#FFFFFF",
                SortOrder = 1,
                Meta = "{}",
                Icons = icons,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ElementCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            // 서버에서 최종 Meta 재조합
            var metaObj = new { description = vm.MetaDescription ?? "", etc = vm.MetaEtc ?? "" };
            vm.Meta = JsonSerializer.Serialize(metaObj);

            var req = new CreateElementRequest(
                vm.Key, vm.Label, vm.IconId, vm.ColorHex, vm.SortOrder, vm.Meta);

            var client = _http.CreateClient("GameApi");
            var resp = await client.PostAsJsonAsync("api/element", req, ct);
            resp.EnsureSuccessStatusCode();

            var id = await resp.Content.ReadFromJsonAsync<int>(cancellationToken: ct);
            TempData["Message"] = "생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // [3] Edit 
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                TempData["Error"] = "잘못된 요청입니다.";
                return RedirectToAction(nameof(Index));
            }

            var client = _http.CreateClient("GameApi");

            // 1) Element 조회
            var resp = await client.GetAsync($"/api/element/{id}", ct);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"Element 조회 실패: {resp.StatusCode}";
                return RedirectToAction(nameof(Index));
            }

            var dto = await resp.Content.ReadFromJsonAsync<Application.Elements.ElementDto>(cancellationToken: ct);
            if (dto == null)
            {
                TempData["Error"] = "Element 데이터를 읽을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            // 2) 아이콘 목록도 함께 조회 (모달 선택용)
            var icons = await client.GetFromJsonAsync<List<IconDto>>("/api/icons", ct)
                         ?? new List<IconDto>();

            // 3) Meta JSON → 분리
            string? desc = null, etc = null;
            if (!string.IsNullOrWhiteSpace(dto.Meta))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(dto.Meta);
                    desc = doc.RootElement.GetProperty("description").GetString();
                    etc = doc.RootElement.GetProperty("etc").GetString();
                }
                catch { /* invalid json이면 무시 */ }
            }

            // 4) ViewModel 매핑
            var vm = new ElementEditVm
            {
                ElementId = dto.ElementId,
                Key = dto.Key,
                Label = dto.Label,
                IconId = dto.IconId,
                IconUrl = dto.IconId.HasValue
                    ? $"{_assetsBaseUrl}/icons/{icons.FirstOrDefault(i => i.IconId == dto.IconId)?.Key}.png"
                    : null,
                ColorHex = dto.ColorHex,
                SortOrder = dto.SortOrder,
                Meta = dto.Meta,
                MetaDescription = desc,
                MetaEtc = etc,
                Icons = icons.Select(x => new IconPickItem
                {
                    IconId = x.IconId,
                    Key = x.Key,
                    Version = x.Version,
                    Url = $"{_assetsBaseUrl}/icons/{x.Key}.png?v={x.Version}"
                }).ToList()
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ElementEditVm vm, CancellationToken ct)
        {
            if (id <= 0 || id != vm.ElementId)
            {
                TempData["Error"] = "잘못된 요청입니다.";
                return RedirectToAction(nameof(Index));
            }

            // 서버에서 Meta를 신뢰성 있게 재조합
            var metaJson = JsonSerializer.Serialize(new
            {
                description = vm.MetaDescription ?? "",
                etc = vm.MetaEtc ?? ""
            });

            if (!ModelState.IsValid)
            {
                // 검증 실패 시 모달용 아이콘 목록/프리뷰를 다시 채워서 화면 유지
                vm.Icons = await LoadIconsAsync(ct);
                if (vm.IconId.HasValue)
                {
                    var sel = vm.Icons.FirstOrDefault(i => i.IconId == vm.IconId.Value);
                    vm.IconUrl = sel?.Url;
                }
                vm.Meta = metaJson; // 화면 재표시 때 hidden에 들어가도록
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");

            // 업데이트 본문 (Key는 변경 안 한다고 가정)
            var req = new UpdateElementRequest(
                Label: vm.Label,
                IconId: vm.IconId,
                ColorHex: vm.ColorHex,
                SortOrder: (short)vm.SortOrder,
                MetaJson: metaJson
            );

            var resp = await client.PutAsJsonAsync($"/api/element/{id}", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                // 실패시 메시지 + 화면 유지
                ModelState.AddModelError(string.Empty, $"수정 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                vm.Icons = await LoadIconsAsync(ct);
                if (vm.IconId.HasValue)
                {
                    var sel = vm.Icons.FirstOrDefault(i => i.IconId == vm.IconId.Value);
                    vm.IconUrl = sel?.Url;
                }
                vm.Meta = metaJson;
                return View(vm);
            }

            TempData["Message"] = "수정되었습니다.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                TempData["Error"] = "잘못된 요청입니다.";
                return RedirectToAction(nameof(Index));

            }

            var client = _http.CreateClient("GameApi");

            // WebServer API에 DELETE 요청
            var res = await client.DeleteAsync($"/api/element/{id}", ct);

            if (!res.IsSuccessStatusCode)
            {
                // 실패하면 오류 메시지 보여주기
                TempData["Error"] = "삭제 실패: " + res.StatusCode;
            }
            else
            {
                TempData["Success"] = "삭제 성공";
            }
            return RedirectToAction(nameof(Index));

        }
    }
}
