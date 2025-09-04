using AdminTool.Models;
using Application.Character;
using Application.Elements;
using Application.Factions;
using Application.Rarities;
using Application.Roles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text.Json;

namespace AdminTool.Controllers
{
    [Route("Characters")]
    public class CharactersController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;
        private readonly string _assetsBaseUrl;
        private readonly string _iconsSubdir; 
        private readonly string _portraitsSubdir;
        public CharactersController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;
            _assetsBaseUrl = cfg["PublicBaseUrl"]!.TrimEnd('/'); // 예: https://localhost:5001/cdn
            _iconsSubdir = cfg["Assets:IconsSubdir"] ?? "icons"; // 기본 폴더명
            _portraitsSubdir = cfg["Assets:PortraitsSubdir"] ?? "portraits";
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] CharacterListFilterVm filter, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // WebServer: GET /api/characters?page=&pageSize=&elementId=&rarityId=&search=
            var url = QueryHelpers.AddQueryString("/api/characters", new Dictionary<string, string?>
            {
                ["page"] = filter.Page.ToString(),
                ["pageSize"] = filter.PageSize.ToString(),
                ["elementId"] = filter.ElementId?.ToString(),
                ["rarityId"] = filter.RarityId?.ToString(),
                ["search"] = filter.Search
            });

            var paged = await client.GetFromJsonAsync<PagedResult<CharacterSummaryDto>>(url, ct)
                        ?? new PagedResult<CharacterSummaryDto>(Array.Empty<CharacterSummaryDto>(), 0, filter.Page, filter.PageSize);

            var vm = new CharacterIndexVm
            {
                Filter = filter,
                Items = paged.Items.Select(CharacterVmMapper.FromDto).ToList(),
                TotalCount = paged.TotalCount
            };

            await PopulateListLookupsAsync(vm.Filter, ct);
            return View(vm);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var dto = await client.GetFromJsonAsync<CharacterDetailDto>($"/api/characters/{id}", ct);
            if (dto is null) return NotFound();

            return View("~/Views/Characters/Details.cshtml", dto);
        }
        [HttpGet("Create")]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new CharacterFormVm();
            await PopulateEditLookupsAsync(vm, ct);
            return View(vm);
        }
        private sealed record IdResponse(int id);

        [HttpPost("Create")]
        public async Task<IActionResult> Create(CharacterFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateEditLookupsAsync(vm, ct);
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");
            var resp = await client.PostAsJsonAsync("/api/characters", vm.ToCreateRequest(), ct);
            if (!resp.IsSuccessStatusCode)
            {
                var raw = await resp.Content.ReadAsStringAsync(ct);

                try
                {
                    // ModelState 기반 400이면 여기로 옴
                    var vpd = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: ct);
                    if (vpd is not null)
                    {
                        foreach (var kv in vpd.Errors)
                            foreach (var msg in kv.Value)
                                ModelState.AddModelError(kv.Key, msg);
                    }
                    else
                    {
                        var pd = await resp.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: ct);
                        if (pd?.Detail is not null)
                            ModelState.AddModelError(string.Empty, pd.Detail);
                        else
                            ModelState.AddModelError(string.Empty, raw);
                    }
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, raw);
                }

                await PopulateEditLookupsAsync(vm, ct);
                return View(vm);
            }

            var created = await resp.Content.ReadFromJsonAsync<IdOnlyDto>(cancellationToken: ct);
            TempData["toast"] = "캐릭터가 생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }
         
        public sealed class IdOnlyDto { public int Id { get; set; } }
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            var resp = await client.GetAsync($"/api/characters/{id}", ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = $"캐릭터(id={id})를 찾을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                // API가 ProblemDetails를 주면 그 detail을 보여주기
                try
                {
                    var pd = JsonSerializer.Deserialize<ProblemDetails>(body);
                    TempData["Error"] = pd?.Detail ?? body;
                }
                catch { TempData["Error"] = body; }

                return RedirectToAction(nameof(Index));
            }

            var dto = JsonSerializer.Deserialize<CharacterDetailDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (dto is null)
            {
                TempData["Error"] = "API 응답을 해석할 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            var vm = CharacterVmMapper.FromDetailDto(dto);
            await PopulateEditLookupsAsync(vm, ct);
            return View(vm);
        }
        [HttpPost("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, CharacterFormVm vm, CancellationToken ct)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateEditLookupsAsync(vm, ct);
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");
            var resp = await client.PutAsJsonAsync($"/api/characters/{id}", vm.ToUpdateRequest(), ct);
            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, $"저장 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                await PopulateEditLookupsAsync(vm, ct);
                return View(vm);
            }

            TempData["toast"] = "기본 정보가 저장되었습니다.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("{id:int}/Skills")]
        public async Task<IActionResult> Skills(int id, CancellationToken ct)
        {
            try
            {
                var client = _http.CreateClient("GameApi");

                // 1) 상태코드 먼저 확인
                var resp = await client.GetAsync($"/api/characters/{id}", ct);
                var body = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                {
                    // ValidationProblemDetails 또는 ProblemDetails 우선 파싱
                    try
                    {
                        var vpd = System.Text.Json.JsonSerializer.Deserialize<ValidationProblemDetails>(body);
                        if (vpd?.Errors?.Count > 0)
                        {
                            foreach (var kv in vpd.Errors)
                                foreach (var msg in kv.Value)
                                    ModelState.AddModelError(kv.Key ?? string.Empty, msg);
                        }
                        else
                        {
                            var pd = System.Text.Json.JsonSerializer.Deserialize<ProblemDetails>(body);
                            TempData["Error"] = pd?.Detail ?? body;
                        }
                    }
                    catch { TempData["Error"] = body; }

                    return RedirectToAction(nameof(Index));
                }

                // 2) DTO 파싱
                var dto = System.Text.Json.JsonSerializer.Deserialize<CharacterDetailDto>(
                    body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (dto is null)
                {
                    TempData["Error"] = "API 응답을 해석할 수 없습니다.";
                    return RedirectToAction(nameof(Index));
                }

                // 3) 스킬 목록 로딩 & VM 구성
                var allSkills = await GetAllSkillsAsync(ct);
                var vm = dto.ToSkillsVm(allSkills);
                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"스킬 편집 화면 로드 실패: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost("{id:int}/Skills")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Skills(int id, CharacterSkillsVm vm, CancellationToken ct)
        {
            if (id != vm.CharacterId) return BadRequest();

            if (!ModelState.IsValid ||
                vm.Rows.GroupBy(r => r.Slot).Any(g => g.Count() > 1))
            {
                if (vm.Rows.GroupBy(r => r.Slot).Any(g => g.Count() > 1))
                    ModelState.AddModelError(string.Empty, "슬롯이 중복되었습니다.");

                vm.AllSkills = await GetAllSkillsAsync(ct);
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");
            var req = vm.ToSkillRequests();
            var resp = await client.PutAsJsonAsync($"/api/characters/{id}/skills", req, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var raw = await resp.Content.ReadAsStringAsync(ct);

                // ValidationProblemDetails 또는 ProblemDetails를 최대한 친절하게 풀어서 표시
                try
                {
                    var vpd = System.Text.Json.JsonSerializer.Deserialize<ValidationProblemDetails>(raw);
                    if (vpd?.Errors?.Count > 0)
                    {
                        foreach (var kv in vpd.Errors)
                            foreach (var msg in kv.Value)
                                ModelState.AddModelError(kv.Key ?? string.Empty, msg);
                    }
                    else
                    {
                        var pd = System.Text.Json.JsonSerializer.Deserialize<ProblemDetails>(raw);
                        ModelState.AddModelError(string.Empty, pd?.Detail ?? raw);
                    }
                }
                catch { ModelState.AddModelError(string.Empty, raw); }

                vm.AllSkills = await GetAllSkillsAsync(ct);
                return View(vm);
            }

            TempData["toast"] = "스킬 세트가 저장되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id:int}/Progressions")]
        public async Task<IActionResult> Progressions(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var dto = await client.GetFromJsonAsync<CharacterDetailDto>($"/api/characters/{id}", ct);
            if (dto is null) return NotFound();
            return View(dto.ToProgressionsVm());
        }

        [HttpPost("{id:int}/Progressions")]
        public async Task<IActionResult> Progressions(int id, CharacterProgressionsVm vm, CancellationToken ct)
        {
            if (id != vm.CharacterId) return BadRequest();
            if (!ModelState.IsValid) return View(vm);
            if (vm.Rows.GroupBy(r => r.Level).Any(g => g.Count() > 1))
            {
                ModelState.AddModelError(string.Empty, "레벨이 중복되었습니다.");
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");
            var req = vm.ToProgressionRequests();
            var resp = await client.PutAsJsonAsync($"/api/characters/{id}/progressions", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, $"저장 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                return View(vm);
            }

            TempData["toast"] = "레벨 스탯이 저장되었습니다.";
            return RedirectToAction(nameof(Progressions), new { id });
        }

        [HttpGet("{id:int}/Promotions")]
        public async Task<IActionResult> Promotions(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var dto = await client.GetFromJsonAsync<CharacterDetailDto>($"/api/characters/{id}", ct);
            if (dto is null) return NotFound();

            var items = await GetAllItemsAsync(ct); // TODO 실제 아이템 리스트로 교체
            return View(dto.ToPromotionsVm(items));
        }


        [HttpPost("{id:int}/Promotions")]
        public async Task<IActionResult> Promotions(int id, CharacterPromotionsVm vm, CancellationToken ct)
        {
            if (id != vm.CharacterId) return BadRequest();
            if (!ModelState.IsValid)
            {
                vm.Items = await GetAllItemsAsync(ct);
                return View(vm);
            }
            if (vm.Rows.GroupBy(r => r.Tier).Any(g => g.Count() > 1))
            {
                ModelState.AddModelError(string.Empty, "티어가 중복되었습니다.");
                vm.Items = await GetAllItemsAsync(ct);
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");
            var req = vm.ToPromotionRequests();
            var resp = await client.PutAsJsonAsync($"/api/characters/{id}/promotions", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, $"저장 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                vm.Items = await GetAllItemsAsync(ct);
                return View(vm);
            }

            TempData["toast"] = "승급 정보가 저장되었습니다.";
            return RedirectToAction(nameof(Promotions), new { id });
        }

        [HttpPost("{id:int}/Delete")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/characters/{id}", ct);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["toast"] = $"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
                return RedirectToAction(nameof(Index));
            }
            TempData["toast"] = "캐릭터가 삭제되었습니다.";
            return RedirectToAction(nameof(Index));
        }
        private async Task PopulateListLookupsAsync(CharacterListFilterVm vm, CancellationToken ct)
        {
            vm.Elements = await BuildElementOptionsAsync(vm.ElementId, ct);
            vm.Rarities = BuildRarityOptions(vm.RarityId);
        }
        private async Task PopulateEditLookupsAsync(CharacterFormVm vm, CancellationToken ct)
        {
            var api = _http.CreateClient("GameApi");

            // 1) Elements
            // 서버 라우트가 /api/element (단수)인지 /api/elements(복수)인지 프로젝트에 맞추세요.
            var elems = await TryGet<List<ElementDto>>(api, "api/element", ct)      // Plan A
                     ?? await TryGet<List<ElementDto>>(api, "api/elements", ct)     // Plan B
                     ?? new();
            vm.Elements = elems
                .OrderBy(e => e.SortOrder).ThenBy(e => e.ElementId)
                .Select(e => new SelectListItem(e.Label, e.ElementId.ToString(), e.ElementId == vm.ElementId))
                .ToList();

            // 2) Rarities
            var rarities = await TryGet<List<RarityDto>>(api, "api/rarities", ct)   // Plan A
                        ?? await TryGet<List<RarityDto>>(api, "api/rarity", ct)     // Plan B
                        ?? new();
            vm.Rarities = rarities
                .OrderBy(r => r.SortOrder).ThenBy(r => r.RarityId)
                .Select(r => new SelectListItem($"{r.Label} (★{r.Stars})", r.RarityId.ToString(), r.RarityId == vm.RarityId))
                .ToList();

            // 3) Roles
            var roles = await TryGet<List<RoleDto>>(api, "api/roles", ct)           // Plan A
                     ?? await TryGet<List<RoleDto>>(api, "api/role", ct)            // Plan B
                     ?? new();
            vm.Roles = roles
                .OrderBy(r => r.SortOrder).ThenBy(r => r.RoleId)
                .Select(r => new SelectListItem(r.Label, r.RoleId.ToString(), r.RoleId == vm.RoleId))
                .ToList();

            // 4) Factions
            var factions = await TryGet<List<FactionDto>>(api, "api/factions", ct)  // Plan A
                        ?? await TryGet<List<FactionDto>>(api, "api/faction", ct)   // Plan B
                        ?? new();
            vm.Factions = factions
                .OrderBy(f => f.SortOrder).ThenBy(f => f.FactionId)
                .Select(f => new SelectListItem(f.Label, f.FactionId.ToString(), f.FactionId == vm.FactionId))
                .ToList();

            // 5) Icons  (이미 Skills에서 쓰던 /api/icons 재활용)
            var icons = await TryGet<List<IconVm>>(api, "api/icons", ct) ?? new();
          
            vm.IconChoices = await LoadIconPickListAsync(ct, vm.IconId);
            vm.Icons = vm.IconChoices
            .Select(i => new SelectListItem(i.Key, i.IconId.ToString(), i.IconId == vm.IconId))
            .ToList();

            // 6) Portraits (서버에 / api / portraits가 있다고 가정.없으면 만들거나 아이콘을 임시 사용)
            vm.PortraitChoices = await LoadPortraitPickListAsync(ct, vm.PortraitId);
            vm.Portraits = vm.PortraitChoices
    .Select(p => new SelectListItem(p.Key, p.PortraitId.ToString(), p.PortraitId == vm.PortraitId))
    .ToList();

            Console.WriteLine($"Charcter : " +
                $"Rarity : {vm.RarityId}," +
                $"ElementId : {vm.ElementId}," +
                $"RoleId : {vm.RoleId}," +
                $"FactionId : {vm.FactionId}," +
                $"Iconid : {vm.IconId}," +
                $"portraitId : {vm.PortraitId}"
                );

        }
        private async Task<List<IconPickItem>> LoadIconPickListAsync(CancellationToken ct, int? selected = null)
        {
            var client = _http.CreateClient("GameApi");
            var apiIcons = await client.GetFromJsonAsync<List<IconVm>>("/api/icons", ct) ?? new();
            var baseUrl = _cfg["PublicBaseUrl"]!.TrimEnd('/');
            var subdir = _cfg["Assets:IconsSubdir"] ?? "icons";

            return apiIcons.Select(x => new IconPickItem
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = $"{baseUrl}/{subdir}/{x.Key}.png?v={x.Version}"
            }).ToList();
        }

        private async Task<List<PortraitPickItem>> LoadPortraitPickListAsync(CancellationToken ct, int? selected = null)
        {
            var client = _http.CreateClient("GameApi");
            var apiPorts = await client.GetFromJsonAsync<List<PortraitVm>>("/api/portraits", ct) ?? new();
            var baseUrl = _cfg["PublicBaseUrl"]!.TrimEnd('/');
            var subdir = _cfg["Assets:PortraitsSubdir"] ?? "portraits";

            return apiPorts.Select(p => new PortraitPickItem
            {
                PortraitId = p.PortraitId,
                Key = p.Key,
                Version = p.Version,
                Url = $"{baseUrl}/{subdir}/{p.Key}.png?v={p.Version}"
            }).ToList();
        }
        private static async Task<T?> TryGet<T>(HttpClient c, string url, CancellationToken ct)
        {
            try
            {
                var resp = await c.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode) return default;
                return await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
            }
            catch
            {
                return default;
            }
        }
        private async Task<IEnumerable<SelectListItem>> BuildElementOptionsAsync(int? selectedId, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            // 기존 SkillsController에서 쓰던 것과 동일: /api/element
            var elements = await client.GetFromJsonAsync<List<Application.Elements.ElementDto>>("/api/element", ct)
                           ?? new();
            return elements.OrderBy(e => e.SortOrder)
                           .Select(e => new SelectListItem(e.Label, e.ElementId.ToString(), e.ElementId == selectedId))
                           .ToList();
        }

        private IEnumerable<SelectListItem> BuildRarityOptions(int? selected)
        {
            // 임시: 1~6 고정. 실제로는 /api/rarities 같은 API 있으면 그걸로 교체
            return Enumerable.Range(1, 6)
                             .Select(v => new SelectListItem($"★{v}", v.ToString(), v == selected))
                             .ToList();
        }

        private async Task<IEnumerable<SelectListItem>> GetAllSkillsAsync(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            // 필요하면 필터/페이지 파라미터 조정
            var list = await client.GetFromJsonAsync<IReadOnlyList<Application.Skills.SkillListItemDto>>("/api/skills?pageSize=500&isActive=true", ct)
                       ?? Array.Empty<Application.Skills.SkillListItemDto>();
            return list.Select(s => new SelectListItem($"{s.Name} (#{s.SkillId})", s.SkillId.ToString()));
        }

        private Task<IEnumerable<SelectListItem>> GetAllItemsAsync(CancellationToken ct)
        {
            // TODO: /api/items 등의 엔드포인트로 교체
            return Task.FromResult<IEnumerable<SelectListItem>>(Enumerable.Empty<SelectListItem>());
        }
    }
}
