using AdminTool.Models;
using Application.Elements;
using Application.SkillLevels;
using Application.Skills;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AdminTool.Controllers
{
    public class SkillsController : Controller
    {
        private readonly IHttpClientFactory _http;

        private readonly string _assetsPhysicalRoot;
        private readonly string _assetsBaseUrl;
        private readonly string _iconsSubdir;

        public SkillsController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;

            _assetsBaseUrl = cfg["PublicBaseUrl"]!.TrimEnd('/');

            _assetsPhysicalRoot = cfg["Assets:PhysicalRoot"]!
                ?? throw new InvalidOperationException("Assets:PhysicalRoot 설정이 필요합니다.");

            _iconsSubdir = cfg["Assets:IconsSubdir"] ?? "icons";
        }

        public async Task<IActionResult> Index(SkillType? type, int? elementId, string? name,
     bool? isActive = null,
     SkillTargetingType? targetingType = null,
     TargetSideType? targetSide = null,
     AoeShapeType? aoeShape = null,
     string[]? tagsAny = null,
     string sortBy = "Name", bool desc = false,
     int page = 1, int pageSize = 50,
     CancellationToken ct = default)
        {
            var client = _http.CreateClient("GameApi");

            var query = new Dictionary<string, string?>
            {
                ["type"] = type?.ToString(),
                ["elementId"] = elementId?.ToString(),
                ["nameContains"] = name,
                ["isActive"] = isActive?.ToString(),
                ["targetingType"] = targetingType?.ToString(),
                ["targetSide"] = targetSide?.ToString(),
                ["aoeShape"] = aoeShape?.ToString(),
                ["sortBy"] = sortBy,
                ["desc"] = desc.ToString(),
                ["page"] = page.ToString(),
                ["pageSize"] = pageSize.ToString()
            };
            if (tagsAny is { Length: > 0 })
                for (int i = 0; i < tagsAny.Length; i++)
                    query[$"tagsAny[{i}]"] = tagsAny[i];

            // 확장 검색 엔드포인트를 쓴다면 /api/skills/search 로
            var apiUrl = QueryHelpers.AddQueryString("/api/skills", query);

            var dtoList = await client.GetFromJsonAsync<IReadOnlyList<SkillListItemDto>>(apiUrl, ct)
                         ?? Array.Empty<SkillListItemDto>();


            // (2) 아이템 목록
            var icons = await client.GetFromJsonAsync<List<IconVm>>("/api/icons", ct) ?? new();
            var iconMap = icons.ToDictionary(k => k.IconId, v => (v.Key, v.Version));
            var itemVms = dtoList.Select(dto =>
            {
                string? iconUrl = null;
                if (iconMap.TryGetValue(dto.IconId, out var info))
                    iconUrl = $"{_assetsBaseUrl}/{_iconsSubdir}/{info.Key}.png?v={info.Version}";

                return SkillListItemVm.From(dto, iconUrl);
            }).ToList();

            var vm = new SkillIndexVm
            {
                Type = type,
                ElementId = elementId,
                NameContains = name,
                // 페이징/정렬/필터도 VM에 매핑해 UI에 반영
                Page = page,
                PageSize = pageSize,
                Items = itemVms,
                TypeOptions = BuildSkillTypeOptions(type),
                ElementOptions = await BuildElementOptionsAsync(elementId, ct),
                TargetingTypeOptions = BuildEnumOptions<SkillTargetingType>(targetingType),
                TargetSideOptions = BuildEnumOptions<TargetSideType>(targetSide),
                AoeShapeOptions = BuildEnumOptions<AoeShapeType>(aoeShape)
            };
            return View(vm);
        }
        private static IReadOnlyList<SelectListItem> BuildEnumOptions<TEnum>(TEnum? selected = null)
    where TEnum : struct, Enum
    => Enum.GetValues(typeof(TEnum))
           .Cast<TEnum>()
           .Select(v => new SelectListItem(v.ToString(), Convert.ToInt32(v).ToString(), selected?.Equals(v) == true))
           .ToList();

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // 1) 아이콘 목록 조회
            var apiIcons = await client.GetFromJsonAsync<List<IconVm>>("/api/icons", ct) ?? new();
            var icons = apiIcons.Select(x => new IconPickItem
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = $"{_assetsBaseUrl}/{_iconsSubdir}/{x.Key}.png?v={x.Version}"
            }).ToList();

            // 2) Elements 조회
            var elements = await client.GetFromJsonAsync<List<ElementDto>>("/api/element", ct) ?? new();

            // 3) ViewModel 생성
            var vm = new SkillCreateVm
            {
                Type = SkillType.Unknown,   // 기본값 예시
                ElementId = elements.FirstOrDefault()?.ElementId ?? 0,
                IconId = icons.FirstOrDefault()?.IconId ?? 0,
                TypeOptions = BuildSkillTypeOptions(null),
                ElementOptions = elements
                    .Select(e => new SelectListItem(e.Label, e.ElementId.ToString()))
                    .ToList(),
                Icons = icons,
                TargetingTypeOptions = BuildSkillTargetOptions(null),
                AoeShapeOptions = BuildAoeShapeType(null),
                TargetSideOptions = BuildSkillSideOptions(null),
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SkillCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                // 실패 시 다시 선택값 로드
                vm.TypeOptions = BuildSkillTypeOptions(vm.Type);
                vm.ElementOptions = await BuildElementOptionsAsync(vm.ElementId, ct);
                vm.Icons = await LoadIconPickListAsync(ct, vm.IconId);
                vm.TargetingTypeOptions = BuildSkillTargetOptions(vm.TargetingType);
                vm.AoeShapeOptions = BuildAoeShapeType(vm.AoeShape);
                vm.TargetSideOptions = BuildSkillSideOptions(vm.TargetSide);
                return View(vm);
            }


            var client = _http.CreateClient("GameApi");

            // API 요청 DTO
            var req = new CreateSkillRequest
            {
                Name = vm.Name,
                Type = vm.Type,
                ElementId = vm.ElementId,
                IconId = vm.IconId,
                IsActive = vm.IsActive,
                TargetingType = vm.TargetingType,
                AoeShape = vm.AoeShape,
                TargetSide = vm.TargetSide,
                Tag = (vm.Tag ?? Array.Empty<string>())
            .Select(t => (t ?? "").Trim().ToLowerInvariant())
            .Where(t => t.Length > 0)
            .Distinct()
            .ToArray(),
                BaseInfo = string.IsNullOrWhiteSpace(vm.Etc) ? null : new JsonObject { ["etc"] = vm.Etc }

            };

            var resp = await client.PostAsJsonAsync("/api/skills", req, ct);

            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, $"생성 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                vm.TypeOptions = BuildSkillTypeOptions(vm.Type);
                vm.ElementOptions = await BuildElementOptionsAsync(vm.ElementId, ct);
                vm.Icons = await LoadIconPickListAsync(ct, vm.IconId);
                vm.TargetingTypeOptions = BuildSkillTargetOptions(vm.TargetingType);
                vm.AoeShapeOptions = BuildAoeShapeType(vm.AoeShape);
                vm.TargetSideOptions = BuildSkillSideOptions(vm.TargetSide);
                return View(vm);
            }

            TempData["Message"] = "스킬이 생성되었습니다.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var s = await client.GetFromJsonAsync<SkillDto>($"/api/skills/{id}", ct);
            if (s is null)
            {
                TempData["Error"] = "Skill을 찾을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            // 드롭다운 옵션
            var typeOpts = BuildSkillTypeOptions(s.Type);
            var elemOpts = await BuildElementOptionsAsync(s.ElementId, ct);
            var targetingOpts = BuildSkillTargetOptions(s.TargetingType);
            var sideOpts = BuildSkillSideOptions(s.TargetSide);
            var aoeOpts = BuildAoeShapeType(s.AoeShape);

            // 아이콘 선택 리스트 + 미리보기
            var icons = await LoadIconPickListAsync(ct, s.IconId);
            var iconUrl = ViewBag.SelectedIconUrl as string;

            // BaseInfo → Etc 추출 (없으면 null)
            string? etc = null;
            if (s.BaseInfo is not null)
            {
                try
                {
                    var node = s.BaseInfo; // JsonNode
                    if (node is System.Text.Json.Nodes.JsonObject obj &&
                        obj.TryGetPropertyValue("etc", out var v) &&
                        v is System.Text.Json.Nodes.JsonValue val &&
                        val.TryGetValue<string>(out var etcStr))
                    {
                        etc = etcStr;
                    }
                }
                catch {  }
            }

            var vm = SkillEditVm.From(
                s,
                typeOpts, elemOpts, targetingOpts, sideOpts, aoeOpts,
                iconUrl: iconUrl,
                icons: icons
            ) with
            { Etc = etc };

            return View(vm); // Views/Skills/Edit.cshtml
        }
        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SkillEditVm vm, CancellationToken ct)
        {
            if (id != vm.SkillId || !ModelState.IsValid)
            {
                ViewBag.IconOptions = await LoadIconPickListAsync(ct, vm.IconId);
                vm = vm with
                {
                    TypeOptions = BuildSkillTypeOptions(vm.Type),
                    ElementOptions = await BuildElementOptionsAsync(vm.ElementId, ct),
                    TargetingTypeOptions = BuildSkillTargetOptions(vm.TargetingType),
                    AoeShapeOptions = BuildAoeShapeType(vm.AoeShape),
                    TargetSideOptions = BuildSkillSideOptions(vm.TargetSide),
            };
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");

            var req = new UpdateSkillBasicsRequest
            {
                Name = vm.Name,
                IconId = vm.IconId
            };

            try
            {
                var resp = await client.PutAsJsonAsync($"/api/skills/{id}", req, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, $"수정 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                    ViewBag.IconOptions = await LoadIconPickListAsync(ct, vm.IconId);
                    vm = vm with
                    {
                        TypeOptions = BuildSkillTypeOptions(vm.Type),
                        ElementOptions = await BuildElementOptionsAsync(vm.ElementId, ct),
                        TargetingTypeOptions = BuildSkillTargetOptions(vm.TargetingType),
                        AoeShapeOptions = BuildAoeShapeType(vm.AoeShape),
                        TargetSideOptions = BuildSkillSideOptions(vm.TargetSide),
                    };
                    return View(vm);
                }

                TempData["Message"] = "저장되었습니다.";
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.IconOptions = await LoadIconPickListAsync(ct, vm.IconId);
                vm = vm with
                {
                    TypeOptions = BuildSkillTypeOptions(vm.Type),
                    ElementOptions = await BuildElementOptionsAsync(vm.ElementId, ct),
                    TargetingTypeOptions = BuildSkillTargetOptions(vm.TargetingType),
                    AoeShapeOptions = BuildAoeShapeType(vm.AoeShape),
                    TargetSideOptions = BuildSkillSideOptions(vm.TargetSide),
                };
                return View(vm);
            }
        }
        [HttpPost("{id:int}/Combat")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCombat(int id, SkillEditVm vm, CancellationToken ct)
        {
            if (id != vm.SkillId) return BadRequest("잘못된 요청입니다.");

            var client = _http.CreateClient("GameApi");
            var req = new UpdateSkillCombatRequest
            {
                Type = vm.Type,
                ElementId = vm.ElementId,
                TargetingType = vm.TargetingType,
                AoeShape = vm.AoeShape,
                TargetSide = vm.TargetSide,
                IsActive = vm.IsActive
            };

            var resp = await client.PutAsJsonAsync($"/api/skills/{id}/combat", req, ct);
            if (!resp.IsSuccessStatusCode)
                return BadRequest($"전투 속성 저장 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");

            TempData["Message"] = "전투 속성이 저장되었습니다.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost("{id:int}/Meta")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMeta(int id, SkillEditVm vm, CancellationToken ct)
        {
            if (id != vm.SkillId) return BadRequest("잘못된 요청입니다.");

            var client = _http.CreateClient("GameApi");
            var req = new PatchSkillMetaRequest
            {
                Tag = (vm.Tag ?? Array.Empty<string>())
            .Select(t => (t ?? "").Trim().ToLowerInvariant())
            .Where(t => t.Length > 0)
            .Distinct()
            .ToArray(),
                BaseInfo = string.IsNullOrWhiteSpace(vm.Etc) ? null : new JsonObject { ["etc"] = vm.Etc },

                NormalizeTags = true
            };

            var resp = await client.PatchAsJsonAsync($"/api/skills/{id}/meta", req, ct);
            if (!resp.IsSuccessStatusCode)
                return BadRequest($"메타 저장 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");

            TempData["Message"] = "메타 정보가 저장되었습니다.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost("{id:int}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            try
            {
                var resp = await client.DeleteAsync($"/api/skills/{id}", ct);
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["Error"] = "삭제 실패: " + resp.StatusCode;
                }
                else
                {
                    TempData["Message"] = "삭제되었습니다.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpGet("{id:int}/Levels")]
        public async Task<IActionResult> Levels(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var items = await client.GetFromJsonAsync<IReadOnlyList<SkillLevelDto>>($"/api/skills/{id}/levels", ct)
                        ?? Array.Empty<SkillLevelDto>();

            var vm = new SkillLevelsVm { SkillId = id, Items = items.ToList() };
            return PartialView("Partials/_SkillLevels", vm);
        }
        [HttpGet("{id:int}/Levels/{level:int}")]
        public async Task<IActionResult> GetLevel(int id, int level, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var dto = await client.GetFromJsonAsync<SkillLevelDto>($"/api/skills/{id}/levels/{level}", ct);
            if (dto is null) return NotFound();

            var vm = new SkillLevelFormVm
            {
                SkillId = dto.SkillId,
                Level = dto.Level,
                ValuesJson = dto.Values is null ? null : JsonSerializer.Serialize(dto.Values),
                Description = dto.Description,
                MaterialsJson = dto.Materials is null ? null : JsonSerializer.Serialize(dto.Materials),
                CostGold = dto.CostGold
            };
            return PartialView("Partials/_EditLevelForm", vm);
        }
        [HttpPost("{id:int}/Levels")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLevel(int id, SkillLevelFormVm vm, CancellationToken ct)
        {
            if (id != vm.SkillId) return BadRequest("잘못된 요청입니다.");

            var req = new CreateSkillLevelRequest
            {
                Level = vm.Level,
                Values = ParseValues(vm.ValuesJson),
                Description = vm.Description,
                Materials = ParseMaterials(vm.MaterialsJson),
                CostGold = vm.CostGold
            };

            var client = _http.CreateClient("GameApi");
            try
            {
                var resp = await client.PostAsJsonAsync($"/api/skills/{id}/levels", req, ct);
                if (!resp.IsSuccessStatusCode)
                    return BadRequest($"생성 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");

                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("{id:int}/Levels/{level:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLevel(int id, int level, SkillLevelFormVm vm, CancellationToken ct)
        {
            if (id != vm.SkillId || level != vm.Level) return BadRequest("잘못된 요청입니다.");

            var req = new UpdateSkillLevelRequest
            {
                Values = ParseValues(vm.ValuesJson),
                Description = vm.Description,
                Materials = ParseMaterials(vm.MaterialsJson),
                CostGold = vm.CostGold
            };

            var client = _http.CreateClient("GameApi");
            try
            {
                var resp = await client.PostAsJsonAsync($"/api/skills/{id}/levels/{level}", req, ct);
                if (!resp.IsSuccessStatusCode)
                    return BadRequest($"수정 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");

                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("{id:int}/Levels/{level:int}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLevel(int id, int level, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            try
            {
                var resp = await client.DeleteAsync($"/api/skills/{id}/levels/{level}", ct);
                if (!resp.IsSuccessStatusCode)
                    return BadRequest($"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");

                return Ok(new { ok = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        private static IDictionary<string, object>? ParseValues(string? json)
            => string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, object>>(json!);

        private static IDictionary<string, int>? ParseMaterials(string? json)
            => string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, int>>(json!);

        private static IReadOnlyList<SelectListItem> BuildSkillTypeOptions(SkillType? selected)
            => Enum.GetValues(typeof(SkillType))
                   .Cast<SkillType>()
                   .Select(t => new SelectListItem(t.ToString(), ((short)t).ToString(), selected == t))
                   .ToList();

        private async Task<IReadOnlyList<SelectListItem>> BuildElementOptionsAsync(int? selectedId, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var elements = await client.GetFromJsonAsync<List<Application.Elements.ElementDto>>("/api/element", ct)
                           ?? new List<Application.Elements.ElementDto>();

            return elements
                .OrderBy(e => e.SortOrder)
                .Select(e => new SelectListItem(e.Label, e.ElementId.ToString(), e.ElementId == selectedId))
                .ToList();
        }
        private static IReadOnlyList<SelectListItem> BuildSkillTargetOptions(SkillTargetingType? selected)
            => Enum.GetValues(typeof(SkillTargetingType))
                   .Cast<SkillTargetingType>()
                   .Select(t => new SelectListItem(t.ToString(), ((short)t).ToString(), selected == t))
                   .ToList();
        private static IReadOnlyList<SelectListItem> BuildAoeShapeType(AoeShapeType? selected)
            => Enum.GetValues(typeof(AoeShapeType))
                   .Cast<AoeShapeType>()
                   .Select(t => new SelectListItem(t.ToString(), ((short)t).ToString(), selected == t))
                   .ToList();
        private static IReadOnlyList<SelectListItem> BuildSkillSideOptions(TargetSideType? selected)
            => Enum.GetValues(typeof(TargetSideType))
                   .Cast<TargetSideType>()
                   .Select(t => new SelectListItem(t.ToString(), ((short)t).ToString(), selected == t))
                   .ToList();
        private async Task<List<IconPickItem>> LoadIconPickListAsync(CancellationToken ct, int? selectedIconId = null)
        {
            var client = _http.CreateClient("GameApi");
            var icons = await client.GetFromJsonAsync<List<IconVm>>("/api/icons", ct) ?? new();

            // ViewBag.IconOptions 용(아이콘 미리보기 URL)
            ViewBag.SelectedIconUrl = selectedIconId.HasValue
                ? icons.Where(i => i.IconId == selectedIconId.Value)
                       .Select(i => $"{_assetsBaseUrl}/{_iconsSubdir}/{i.Key}.png?v={i.Version}")
                       .FirstOrDefault()
                : null;

            return icons.Select(i => new IconPickItem
            {
                IconId = i.IconId,
                Key = i.Key,
                Version = i.Version,
                Url = $"{_assetsBaseUrl}/{_iconsSubdir}/{i.Key}.png?v={i.Version}"
            }).ToList();
        }
    }
}
