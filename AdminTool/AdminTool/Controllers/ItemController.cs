using AdminTool.Models;
using Application.Common.Models;
using Application.Items;
using Application.Rarities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using static AdminTool.Controllers.IconsController;
using static AdminTool.Controllers.PortraitsController;

namespace AdminTool.Controllers
{
    public sealed class ItemsController : Controller
    {
        private readonly IHttpClientFactory _http;
        public ItemsController(IHttpClientFactory http) => _http = http;

        private HttpClient Api => _http.CreateClient("GameApi");

        // =============== Index (List + Filter) ===============
        [HttpGet("/admin/items")]
        public async Task<IActionResult> Index([FromQuery] ItemListFilterVm filter, CancellationToken ct)
        {
            var qs = ToQuery(filter);
            var page = await Api.GetFromJsonAsync<PagedResult<ItemDto>>($"/api/items{qs}", ct)
                       ?? new PagedResult<ItemDto>(Array.Empty<ItemDto>(), 1, filter.PageSize, 0);

            var rows = page.Items.Select(ItemVm.From).ToList();
            ViewBag.Page = page;
            ViewBag.Filter = filter;
            return View(rows);
        }
        private async Task<List<T>> GetListOrEmpty<T>(HttpClient client, string url, CancellationToken ct)
        {
            try
            {
                using var resp = await client.GetAsync(url, ct);
                if (resp.StatusCode == HttpStatusCode.NotFound) return new List<T>(); // 404 → 빈 목록
                resp.EnsureSuccessStatusCode();
                var data = await resp.Content.ReadFromJsonAsync<List<T>>(cancellationToken: ct);
                return data ?? new List<T>();
            }
            catch (Exception ex)
            {
                // 실패해도 폼은 뜨게

                return new List<T>();
            }
        }
        [HttpGet("/admin/items/new")]
        public async Task<IActionResult> New(CancellationToken ct)
        {
            await PopulateLookups(ct);
            return View("Create", new ItemEditVm());
        }
        private async Task<List<T>> GetPagedItemsOrEmpty<T>(HttpClient client, string url, CancellationToken ct)
        {
            try
            {
                var env = await client.GetFromJsonAsync<PagedEnvelope<T>>(url, ct);
                return env?.Items ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }
        private sealed class PagedEnvelope<T>
        {
            public List<T>? Items { get; set; }
        }
        private sealed record CurrencyDto(short Id, string Code, string Name);
        private sealed record StatTypeDto(int Id, string Code, string Name, bool IsPercent);
        private sealed record ItemTypeDto(int Id, string Code, string Name);
        
        [HttpPost("/admin/items/new")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(ItemEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookups(ct);      
                return View("Create", vm);            
            }

            var req = vm.ToCreateRequest(User?.Identity?.Name ?? "admin");
            var resp = await Api.PostAsJsonAsync("/api/items", req, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                await PopulateLookups(ct);
                TempData["Error"] = $"생성 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
                return View("Create", vm);
            }
            var created = await resp.Content.ReadFromJsonAsync<ItemDto>(cancellationToken: ct);
            TempData["Message"] = $"[{vm.Code}] 생성 완료";
            return RedirectToAction(nameof(Index));
        }
        private async Task PopulateLookups(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            var typesTask = GetPagedItemsOrEmpty<ItemTypeDto>(client, "/api/itemtypes?page=1&pageSize=1000", ct);
            var raritiesTask = GetListOrEmpty<RarityDto>(client, "/api/rarities", ct);
            var iconsTask = GetListOrEmpty<IconApiDto>(client, "/api/icons", ct);
            var portraitsTask = GetListOrEmpty<PortraitApiDto>(client, "/api/portraits", ct);
            var statsTask = GetListOrEmpty<StatTypeDto>(client, "/api/stattypes", ct);
            var currenciesTask = GetListOrEmpty<CurrencyDto>(client, "/api/currencies", ct);
            await Task.WhenAll(typesTask, raritiesTask, iconsTask, portraitsTask, statsTask, currenciesTask);

            ViewBag.TypeOptions = (typesTask.Result ?? new())
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem($"{x.Name} (#{x.Id})", x.Id.ToString()))
                .ToList();

            ViewBag.RarityOptions = (raritiesTask.Result ?? new())
                .OrderBy(x => x.SortOrder)
                .Select(x => new SelectListItem($"{x.Label} ★{x.Stars}", x.RarityId.ToString()))
                .ToList();

            var iconItems = new List<SelectListItem> { new("(none)", "") };
            iconItems.AddRange((iconsTask.Result ?? new())
                .OrderBy(x => x.Key)
                .Select(x => new SelectListItem($"{x.Key} (#{x.IconId})", x.IconId.ToString())));
            ViewBag.IconOptions = iconItems;

            var portraitItems = new List<SelectListItem> { new("(none)", "") };
            portraitItems.AddRange((portraitsTask.Result ?? new())
                .OrderBy(x => x.Key)
                .Select(x => new SelectListItem($"{x.Key} (#{x.PortraitId})", x.PortraitId.ToString())));
            ViewBag.PortraitOptions = portraitItems;

            ViewBag.StatOptions = (statsTask.Result ?? new())
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem(
                    text: $"{s.Name} ({s.Code}){(s.IsPercent ? " %" : "")}",
                    value: s.Id.ToString()))
                .ToList();
            ViewBag.CurrencyOptions = (currenciesTask.Result ?? new())
                .OrderBy(c => c.Code)
                .Select(c => new SelectListItem($"{c.Code} - {c.Name}", c.Id.ToString()))
                .ToList();
        }

        // =============== Edit (기본정보) ===============
        [HttpGet("/admin/items/{id:long}")]
        public async Task<IActionResult> Edit(long id, CancellationToken ct)
        {
            var dto = await Api.GetFromJsonAsync<ItemDto>($"/api/items/{id}", ct);
            if (dto is null) { TempData["Error"] = "아이템을 찾을 수 없습니다."; return RedirectToAction(nameof(Index)); }
            return View(ItemEditVm.From(dto));
        }

        [HttpPost("/admin/items/{id:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, ItemEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(vm);

            var req = vm.ToUpdateRequest();
            var resp = await Api.PatchAsJsonAsync("/api/items", req, ct);   // PATCH /api/items

            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"수정 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
                return View(vm);
            }

            TempData["Message"] = $"[{vm.Code}] 수정 완료";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // =============== Child: Stats ===============
        [HttpPost("/admin/items/{id:long}/stats")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpsertStat(long id, ItemStatVm vm, CancellationToken ct)
        {
            var req = vm.ToRequest();
            var resp = await Api.PutAsJsonAsync($"/api/items/{id}/stats", req, ct);
            await NotifyAndRedirect(id, resp, "스탯 저장", ct);
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost("/admin/items/{id:long}/stats/{statId:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStat(long id, int statId, CancellationToken ct)
        {
            var resp = await Api.DeleteAsync($"/api/items/{id}/stats/{statId}", ct);
            await NotifyAndRedirect(id, resp, "스탯 삭제", ct);
            return RedirectToAction(nameof(Edit), new { id });
        }

        // =============== Child: Effects ===============
        [HttpPost("/admin/items/{id:long}/effects/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEffect(long id, ItemEffectVm vm, CancellationToken ct)
        {
            var req = vm.ToAddRequest();
            var resp = await Api.PostAsJsonAsync($"/api/items/{id}/effects", req, ct);
            await NotifyAndRedirect(id, resp, "효과 추가", ct);
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost("/admin/items/{id:long}/effects/{effectId:long}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEffect(long id, long effectId, ItemEffectVm vm, CancellationToken ct)
        {
            var req = vm.ToUpdateRequest(id);
            var resp = await Api.PatchAsJsonAsync($"/api/items/{id}/effects/{effectId}", req, ct);
            await NotifyAndRedirect(id, resp, "효과 수정", ct);
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost("/admin/items/{id:long}/effects/{effectId:long}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEffect(long id, long effectId, CancellationToken ct)
        {
            var resp = await Api.DeleteAsync($"/api/items/{id}/effects/{effectId}", ct);
            await NotifyAndRedirect(id, resp, "효과 삭제", ct);
            return RedirectToAction(nameof(Edit), new { id });
        }

        // =============== Child: Prices ===============
        [HttpPost("/admin/items/{id:long}/prices")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrice(long id, ItemPriceVm vm, CancellationToken ct)
        {
            var req = vm.ToRequest();
            var resp = await Api.PutAsJsonAsync($"/api/items/{id}/prices", req, ct);
            await NotifyAndRedirect(id, resp, "가격 저장", ct);
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost("/admin/items/{id:long}/prices/{currencyId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePrice(long id, int currencyId, ItemPriceVm vm, CancellationToken ct)
        {
            var qs = $"?priceType={vm.PriceType}";
            var resp = await Api.DeleteAsync($"/api/items/{id}/prices/{currencyId}{qs}", ct);
            await NotifyAndRedirect(id, resp, "가격 삭제", ct);
            return RedirectToAction(nameof(Edit), new { id });
        }

        // =============== Delete ===============
        [HttpPost("/admin/items/{id:long}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            var resp = await Api.DeleteAsync($"/api/items/{id}", ct);
            if (resp.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.OK)
                TempData["Message"] = "삭제 완료";
            else
                TempData["Error"] = $"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- helpers ----------------
        private static string ToQuery(ItemListFilterVm f)
        {
            var q = new List<string>();
            if (f.TypeId is { } t) q.Add($"typeId={t}");
            if (f.RarityId is { } r) q.Add($"rarityId={r}");
            if (f.IsActive is { } a) q.Add($"isActive={a.ToString().ToLower()}");
            if (!string.IsNullOrWhiteSpace(f.Search)) q.Add($"search={Uri.EscapeDataString(f.Search)}");
            if (f.Tags is { Length: > 0 })
                q.AddRange(f.Tags.Select(t => $"tags={Uri.EscapeDataString(t)}"));
            if (!string.IsNullOrWhiteSpace(f.Sort)) q.Add($"sort={f.Sort}");
            q.Add($"page={Math.Max(1, f.Page)}");
            q.Add($"pageSize={Math.Clamp(f.PageSize, 1, 500)}");
            return q.Count > 0 ? "?" + string.Join("&", q) : "";
        }

        private async Task NotifyAndRedirect(long id, HttpResponseMessage resp, string action, CancellationToken ct)
        {
            if (resp.IsSuccessStatusCode)
            {
                TempData["Message"] = $"{action} 완료";
                return;
            }
            var body = await resp.Content.ReadAsStringAsync(ct);
            TempData["Error"] = $"{action} 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
        }
    }
}
