using AdminTool.Models;
using AdminTool.Services;
using Application.Roles;
using Microsoft.AspNetCore.Mvc;

namespace AdminTool.Controllers
{
    public class RolesController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IAdminAssetUrlBuilder _assetUrl;
        private readonly IAdminAssetCatalog _catalog;

        public RolesController(IHttpClientFactory http, IAdminAssetUrlBuilder assetUrl, IAdminAssetCatalog catalog)
        {
            _http = http;
            _assetUrl = assetUrl;
            _catalog = catalog;
        }
        // GET: /Roles
        public async Task<IActionResult> Index(bool? isActive, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            
            // (1) Roles 조회 
            var url = "/api/roles";
            if (isActive != null) url += $"?isActive={isActive.Value.ToString().ToLower()}";

            var list = await client.GetFromJsonAsync<List<RoleDto>>(url, ct)
                       ?? new List<RoleDto>();

            // (2) Icons 조회
            var icons = await _catalog.GetIconsAsync(ct);

            var iconMap = icons.ToDictionary(k => k.IconId, v => (v.Key, v.Version));

            // (3) 모델 구성
            var model = list.Select(x =>
            {
                string? iconUrl = null;
                if (x.IconId.HasValue && iconMap.TryGetValue(x.IconId.Value, out var info))
                {
                    iconUrl = _assetUrl.Icon(info.Key, info.Version); 
                }
                return new RoleVm
                {
                    RoleId = x.RoleId,
                    Key = x.Key,
                    Label = x.Label,
                    ColorHex = x.ColorHex,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    Meta = x.Meta,
                    IconUrl = iconUrl
                };
            }).ToList();
            
            ViewBag.FilterIsActive = isActive;
            return View(model);
        }

        // GET: /Roles/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // 아이콘을 조회
            var apiIcons = await _catalog.GetIconsAsync(ct);

            var icons = apiIcons.Select(x => new IconPickItem
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = _assetUrl.Icon(x.Key, x.Version)
            }).ToList();

            var vm = new RoleCreateVm
            {
                ColorHex = "#FFFFFF",
                SortOrder = 1,
                Meta = "",
                Icons = icons,
            };
            return View(vm);
        }

        // POST: /Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var client = _http.CreateClient("GameApi");
            var req = new CreateRoleRequest
            {
                Key = vm.Key,
                Label = vm.Label,
                ColorHex = vm.ColorHex,
                SortOrder = vm.SortOrder,
                IsActive = vm.IsActive,
                IconId = vm.IconId,
                Meta = vm.Meta
            };

            var resp = await client.PostAsJsonAsync("/api/roles", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"생성 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
                return View(vm);
            }

            TempData["Message"] = "Role이 생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Roles/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var dto = await client.GetFromJsonAsync<RoleDto>($"/api/roles/{id}", ct);
            if (dto == null) return NotFound();

            // 아이콘을 조회
            var apiIcons = await _catalog.GetIconsAsync(ct);

            var icons = apiIcons.Select(x => new IconPickItem
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = _assetUrl.Icon(x.Key, x.Version)
            }).ToList();

            var vm = new RoleEditVm
            {
                RoleId = dto.RoleId,
                Label = dto.Label,
                ColorHex = dto.ColorHex,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                Meta = dto.Meta,
                Icons = icons,
            };
            return View(vm);
        }

        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleEditVm vm, CancellationToken ct)
        {

            if (!ModelState.IsValid)
            {
                var icons = (await _catalog.GetIconsAsync(ct))
                    .Select(x => new IconPickItem
                    {
                        IconId = x.IconId,
                        Key = x.Key,
                        Version = x.Version,
                        Url = _assetUrl.Icon(x.Key, x.Version)
                    }).ToList();

                vm.Icons = icons;
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");
            var req = new UpdateRoleRequest
            {
                Label = vm.Label,
                ColorHex = vm.ColorHex,
                SortOrder = vm.SortOrder,
                IsActive = vm.IsActive,
                IconId = vm.IconId,
                Meta = vm.Meta
            };

            var resp = await client.PutAsJsonAsync($"/api/roles/{id}", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"수정 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
                return View(vm);
            }
            TempData["Message"] = "Role이 수정되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Roles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/roles/{id}", ct);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
            }
            else
            {
                TempData["Message"] = "Role이 삭제되었습니다.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
