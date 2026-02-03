using AdminTool.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AdminTool.Controllers
{
    public sealed class ShopController : Controller
    {
        private readonly IHttpClientFactory _http;
        public ShopController(IHttpClientFactory http) => _http = http;

        private HttpClient Api => _http.CreateClient("GameApi");

        // ═══════ 상점 목록 ═══════

        [HttpGet("/admin/shop")]
        public async Task<IActionResult> Index([FromQuery] ShopListFilterVm filter, CancellationToken ct)
        {
            var qs = BuildShopQuery(filter);
            var page = await Api.GetFromJsonAsync<PagedResultJson<ShopVm>>($"/api/shop{qs}", ct);

            ViewBag.Page = page;
            ViewBag.Filter = filter;
            return View(page?.Items ?? new List<ShopVm>());
        }

        // ═══════ 상점 생성 ═══════

        [HttpGet("/admin/shop/new")]
        public IActionResult Create() => View(new ShopEditVm());

        [HttpPost("/admin/shop/new")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShopEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var req = new
            {
                vm.Code,
                vm.Name,
                vm.ShopType,
                vm.StartsAt,
                vm.EndsAt,
                vm.IsActive,
                vm.SortOrder
            };

            var resp = await Api.PostAsJsonAsync("/api/shop", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"생성 실패: {(int)resp.StatusCode} {body}";
                return View(vm);
            }

            TempData["Message"] = $"[{vm.Code}] 상점 생성 완료";
            return RedirectToAction(nameof(Index));
        }

        // ═══════ 상점 수정 ═══════

        [HttpGet("/admin/shop/{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var dto = await Api.GetFromJsonAsync<ShopEditJson>($"/api/shop/{id}", ct);
            if (dto is null)
            {
                TempData["Error"] = "상점을 찾을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new ShopEditVm
            {
                Id = dto.Id,
                Code = dto.Code,
                Name = dto.Name,
                ShopType = dto.ShopType,
                StartsAt = dto.StartsAt,
                EndsAt = dto.EndsAt,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder,
                Products = dto.Products ?? new()
            };

            return View(vm);
        }

        [HttpPost("/admin/shop/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShopEditVm vm, CancellationToken ct)
        {
            vm.Id = id;
            if (!ModelState.IsValid)
            {
                // 상품 목록 다시 로드
                var detail = await Api.GetFromJsonAsync<ShopEditJson>($"/api/shop/{id}", ct);
                vm.Products = detail?.Products ?? new();
                return View(vm);
            }

            var req = new
            {
                vm.Name,
                vm.ShopType,
                vm.StartsAt,
                vm.EndsAt,
                vm.IsActive,
                vm.SortOrder
            };

            var resp = await Api.PutAsJsonAsync($"/api/shop/{id}", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"수정 실패: {(int)resp.StatusCode} {body}";
                var detail = await Api.GetFromJsonAsync<ShopEditJson>($"/api/shop/{id}", ct);
                vm.Products = detail?.Products ?? new();
                return View(vm);
            }

            TempData["Message"] = "상점 수정 완료";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // ═══════ 상점 삭제 ═══════

        [HttpPost("/admin/shop/{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var resp = await Api.DeleteAsync($"/api/shop/{id}", ct);
            TempData[resp.IsSuccessStatusCode ? "Message" : "Error"] =
                resp.IsSuccessStatusCode ? "상점 삭제 완료" : $"삭제 실패: {(int)resp.StatusCode}";
            return RedirectToAction(nameof(Index));
        }

        // ═══════ 상품 추가 ═══════

        [HttpPost("/admin/shop/{shopId:int}/products/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(int shopId, ShopProductEditVm vm, CancellationToken ct)
        {
            var req = new
            {
                vm.ItemId,
                vm.CurrencyId,
                vm.Price,
                vm.QuantityPerPurchase,
                vm.DailyLimit,
                vm.WeeklyLimit,
                vm.TotalLimit,
                vm.SortOrder,
                vm.IsActive
            };

            var resp = await Api.PostAsJsonAsync($"/api/shop/{shopId}/products", req, ct);
            TempData[resp.IsSuccessStatusCode ? "Message" : "Error"] =
                resp.IsSuccessStatusCode ? "상품 추가 완료" : $"상품 추가 실패: {(int)resp.StatusCode}";
            return RedirectToAction(nameof(Edit), new { id = shopId });
        }

        // ═══════ 상품 수정 ═══════

        [HttpPost("/admin/shop/{shopId:int}/products/{productId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(
            int shopId, int productId, ShopProductEditVm vm, CancellationToken ct)
        {
            var req = new
            {
                vm.ItemId,
                vm.CurrencyId,
                vm.Price,
                vm.QuantityPerPurchase,
                vm.DailyLimit,
                vm.WeeklyLimit,
                vm.TotalLimit,
                vm.SortOrder,
                vm.IsActive
            };

            var resp = await Api.PutAsJsonAsync($"/api/shop/{shopId}/products/{productId}", req, ct);
            TempData[resp.IsSuccessStatusCode ? "Message" : "Error"] =
                resp.IsSuccessStatusCode ? "상품 수정 완료" : $"상품 수정 실패: {(int)resp.StatusCode}";
            return RedirectToAction(nameof(Edit), new { id = shopId });
        }

        // ═══════ 상품 삭제 ═══════

        [HttpPost("/admin/shop/{shopId:int}/products/{productId:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int shopId, int productId, CancellationToken ct)
        {
            var resp = await Api.DeleteAsync($"/api/shop/{shopId}/products/{productId}", ct);
            TempData[resp.IsSuccessStatusCode ? "Message" : "Error"] =
                resp.IsSuccessStatusCode ? "상품 삭제 완료" : $"상품 삭제 실패: {(int)resp.StatusCode}";
            return RedirectToAction(nameof(Edit), new { id = shopId });
        }

        // ═══════ 구매 기록 ═══════

        [HttpGet("/admin/shop/logs")]
        public async Task<IActionResult> Logs([FromQuery] PurchaseLogFilterVm filter, CancellationToken ct)
        {
            var qs = BuildLogQuery(filter);
            var page = await Api.GetFromJsonAsync<PagedResultJson<PurchaseLogVm>>(
                $"/api/shop/purchase-logs{qs}", ct);

            ViewBag.Page = page;
            ViewBag.Filter = filter;
            return View(page?.Items ?? new List<PurchaseLogVm>());
        }

        // ═══════ Helper ═══════

        private static string BuildShopQuery(ShopListFilterVm f)
        {
            var q = new List<string>();
            if (f.ShopType.HasValue) q.Add($"shopType={f.ShopType}");
            if (f.IsActive.HasValue) q.Add($"isActive={f.IsActive.Value.ToString().ToLower()}");
            if (!string.IsNullOrWhiteSpace(f.Search)) q.Add($"search={Uri.EscapeDataString(f.Search)}");
            q.Add($"page={Math.Max(1, f.Page)}");
            q.Add($"pageSize={Math.Clamp(f.PageSize, 1, 100)}");
            return q.Count > 0 ? "?" + string.Join("&", q) : "";
        }

        private static string BuildLogQuery(PurchaseLogFilterVm f)
        {
            var q = new List<string>();
            if (f.UserId.HasValue) q.Add($"userId={f.UserId}");
            if (f.ShopProductId.HasValue) q.Add($"shopProductId={f.ShopProductId}");
            if (f.From.HasValue) q.Add($"from={Uri.EscapeDataString(f.From.Value.ToString("o"))}");
            if (f.To.HasValue) q.Add($"to={Uri.EscapeDataString(f.To.Value.ToString("o"))}");
            q.Add($"page={Math.Max(1, f.Page)}");
            q.Add($"pageSize={Math.Clamp(f.PageSize, 1, 200)}");
            return q.Count > 0 ? "?" + string.Join("&", q) : "";
        }

        // API JSON 응답 역직렬화용 내부 타입
        private sealed class PagedResultJson<T>
        {
            public List<T> Items { get; set; } = new();
            public int Page { get; set; }
            public int PageSize { get; set; }
            public long TotalCount { get; set; }
            public int TotalPages { get; set; }
            public bool HasPrev { get; set; }
            public bool HasNext { get; set; }
        }

        private sealed class ShopEditJson
        {
            public int Id { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public int ShopType { get; set; }
            public DateTimeOffset? StartsAt { get; set; }
            public DateTimeOffset? EndsAt { get; set; }
            public bool IsActive { get; set; }
            public int SortOrder { get; set; }
            public List<ShopProductVm> Products { get; set; } = new();
        }
    }
}
