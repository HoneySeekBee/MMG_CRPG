using AdminTool.Models;
using Application.Synergy;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Domain.Entities;
using System.Text.Json.Serialization;

namespace AdminTool.Controllers
{
    [Route("Synergies")]
    public class SynergiesController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;


        private readonly string _assetsBaseUrl;  // 예: https://localhost:5001/cdn
        private readonly string _iconsSubdir;
        private readonly string _portraitsSubdir;

        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };

        public SynergiesController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;

            _assetsBaseUrl = (cfg["PublicBaseUrl"] ?? "").TrimEnd('/');
            _iconsSubdir = cfg["Assets:IconsSubdir"] ?? "icons";
            _portraitsSubdir = cfg["Assets:PortraitsSubdir"] ?? "portraits";
        }

        private HttpClient Api() => _http.CreateClient("GameApi");
        private HttpClient Client() => _http.CreateClient();

        // 목록
        [HttpGet("")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            // 상대 경로 사용 (BaseAddress가 Program.cs에서 이미 설정됨)
            var list = await Api().GetFromJsonAsync<IReadOnlyList<SynergyDto>>(
                           "api/synergies/actives", _json, ct)
                       ?? Array.Empty<SynergyDto>();
            return View(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details([FromRoute] int id, CancellationToken ct)
        {
            var dto = await Api().GetFromJsonAsync<SynergyDto>($"api/synergies/{id}", _json, ct);
            if (dto is null) return NotFound();
            return View(dto);
        }
        private sealed class ElementVm { public int ElementId { get; set; } public string Name { get; set; } = ""; }
        private sealed class FactionVm { public int FactionId { get; set; } public string Name { get; set; } = ""; }
        private sealed class ItemTagVm { public int TagId { get; set; } public string Name { get; set; } = ""; }
        private sealed class ElementLookupDto
        {
            [JsonPropertyName("elementId")] public int ElementId { get; set; }
            [JsonPropertyName("label")] public string? Label { get; set; }
            [JsonPropertyName("key")] public string? Key { get; set; }
        }

        private sealed class FactionLookupDto
        {
            [JsonPropertyName("factionId")] public int FactionId { get; set; }
            [JsonPropertyName("label")] public string? Label { get; set; }
            [JsonPropertyName("key")] public string? Key { get; set; }
        }
        private async Task LoadLookupsAsync(SynergyEditVm vm, CancellationToken ct)
        {
            var client = Api();

            // 1) 아이콘
            var apiIcons = await client.GetFromJsonAsync<List<IconVm>>("api/icons", ct) ?? new();
            var baseUrl = _assetsBaseUrl?.TrimEnd('/');
            var iconDir = string.IsNullOrWhiteSpace(_iconsSubdir) ? "icons" : _iconsSubdir.Trim('/');

            vm.Icons = apiIcons.Select(x => new IconPickItem
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = $"{baseUrl}/{iconDir}/{x.Key}.png?v={x.Version}"
            }).ToList();

            // 2) StatTypes
            var apiStats = await client.GetFromJsonAsync<List<StatTypeVm>>("api/stattypes", ct) ?? new();
            vm.StatTypes = apiStats
                .OrderBy(s => s.Id)
                .Select(s => new StatTypePickItem { Code = s.Code, Name = s.Name, IsPercent = s.IsPercent })
                .ToList();

            // 3) Rules 드롭다운용 목록
            var els = await client.GetFromJsonAsync<List<ElementLookupDto>>("api/element", ct) ?? new();
            var facs = await client.GetFromJsonAsync<List<FactionLookupDto>>("api/factions", ct) ?? new();

            vm.Elements = els.Select(x => new PickItem
            {
                Id = x.ElementId,
                Name = x.Label ?? x.Key ?? x.ElementId.ToString()
            }).ToList();

            vm.Factions = facs.Select(x => new PickItem
            {
                Id = x.FactionId,
                Name = x.Label ?? x.Key ?? x.FactionId.ToString()
            }).ToList();

            // 뷰에서 경로 쓸 경우
            ViewBag.IconsDir = $"{baseUrl}/{iconDir}";
            ViewBag.AssetsBaseUrl = _assetsBaseUrl;
            ViewBag.PortraitsDir = $"{_assetsBaseUrl}/{_portraitsSubdir}";
        }
        private async Task<List<T>> TryGetAsync<T>(string url, CancellationToken ct)
        {
            try
            {
                var resp = await Api().GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode) return new List<T>();
                var data = await resp.Content.ReadFromJsonAsync<List<T>>(_json, ct);
                return data ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }
        private static DateTime? ToUtcFromLocal(DateTime? dt)
        {
            if (dt == null) return null;
            var v = dt.Value;
            // datetime-local 은 Kind=Unspecified 로 들어옴 → 로컬로 간주 후 Utc 변환
            if (v.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(v, DateTimeKind.Local).ToUniversalTime();
            return v.ToUniversalTime();
        }
        [HttpGet("Create")]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new SynergyEditVm
            {
                EffectJson = """{ "modifiers":[{ "stat":"ATK","op":"add_pct","value":10 }] }""",
                Stacking = 0,
                IsActive = true
            };

            await LoadLookupsAsync(vm, ct);
            return View(vm);
        }
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SynergyEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync(vm, ct);
                return View(vm);
            }

            if (!TryParseJson(vm.EffectJson, out var effect))
            {
                ModelState.AddModelError(nameof(vm.EffectJson), "유효한 JSON이 아닙니다.");
                await LoadLookupsAsync(vm, ct);
                return View(vm);
            }

            // 먼저 UTC로 정규화
            vm.StartAt = ToUtcFromLocal(vm.StartAt);
            vm.EndAt = ToUtcFromLocal(vm.EndAt);

            // 그 다음 req 생성

            var req = new CreateSynergyRequest(
                vm.Key, vm.Name, vm.Description, vm.IconId,
                effect!, vm.Stacking, vm.IsActive, vm.StartAt, vm.EndAt,
                Bonuses: vm.Bonuses.Select(b =>
                    new CreateSynergyBonusRequest(b.Threshold, JsonDocument.Parse(b.EffectJson), b.Note)).ToList(),
                Rules: vm.Rules.Select(r =>
                    new CreateSynergyRuleRequest(r.Scope, r.Metric, r.RefId, r.RequiredCnt,
                        string.IsNullOrWhiteSpace(r.ExtraJson) ? null : JsonDocument.Parse(r.ExtraJson))).ToList()
            );

            var resp = await Api().PostAsJsonAsync("api/synergies", req, _json, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty,
         $"생성 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{body}");
                
                await LoadLookupsAsync(vm, ct);
                return View(vm);
            }

            var created = await resp.Content.ReadFromJsonAsync<SynergyDto>(_json, ct);
            TempData["ok"] = "시너지가 생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit([FromRoute] int id, CancellationToken ct)
        {
            var s = await Api().GetFromJsonAsync<SynergyDto>($"api/synergies/{id}", _json, ct);
            if (s is null) return NotFound();

            var vm = new SynergyEditVm
            {
                SynergyId = s.SynergyId,
                Key = s.Key,
                Name = s.Name,
                Description = s.Description,
                IconId = s.IconId,
                EffectJson = s.Effect.RootElement.GetRawText(),
                Stacking = s.Stacking,
                IsActive = s.IsActive,
                StartAt = s.StartAt,
                EndAt = s.EndAt,
                Bonuses = s.Bonuses.Select(b => new BonusVm
                {
                    Threshold = b.Threshold,
                    EffectJson = b.Effect.RootElement.GetRawText(),
                    Note = b.Note
                }).ToList(),
                Rules = s.Rules.Select(r => new RuleVm
                {
                    Scope = r.Scope,
                    Metric = r.Metric,
                    RefId = r.RefId,
                    RequiredCnt = r.RequiredCnt,
                    ExtraJson = r.Extra?.RootElement.GetRawText()
                }).ToList()
            };

            await LoadLookupsAsync(vm, ct);
            return View(vm);
        }
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int id, SynergyEditVm vm, CancellationToken ct)
        {
            if (id != vm.SynergyId) return BadRequest();

            JsonDocument? effect = null;
            if (!string.IsNullOrWhiteSpace(vm.EffectJson) && !TryParseJson(vm.EffectJson, out effect))
            {
                ModelState.AddModelError(nameof(vm.EffectJson), "유효한 JSON이 아닙니다.");
                await LoadLookupsAsync(vm, ct);
                return View(vm);
            }

            // 먼저 UTC로 정규화
            vm.StartAt = ToUtcFromLocal(vm.StartAt);
            vm.EndAt = ToUtcFromLocal(vm.EndAt);

            // 그 다음 req 생성
            var req = new UpdateSynergyRequest(
                id, vm.Name, vm.Description, vm.IconId,
                effect, vm.Stacking, vm.IsActive, vm.StartAt, vm.EndAt
            );

            var resp = await Api().PutAsJsonAsync($"api/synergies/{id}", req, _json, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();

                ModelState.AddModelError(string.Empty,
                    $"수정 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{body}");
                
                await LoadLookupsAsync(vm, ct);
                return View(vm);
            }

            TempData["ok"] = "저장되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        // 삭제
        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var resp = await Api().DeleteAsync($"api/synergies/{id}", ct);
            if (!resp.IsSuccessStatusCode)
            {
                TempData["err"] = $"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
                return RedirectToAction(nameof(Edit), new { id });
            }
            TempData["ok"] = "삭제되었습니다.";
            return RedirectToAction(nameof(Index));
        }

        private static bool TryParseJson(string? json, out JsonDocument? doc)
        {
            doc = null;
            if (string.IsNullOrWhiteSpace(json)) return false;
            try { doc = JsonDocument.Parse(json); return true; } catch { return false; }
        }

    }
}
