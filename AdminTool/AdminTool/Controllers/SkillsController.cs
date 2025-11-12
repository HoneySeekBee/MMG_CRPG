using AdminTool.Models;
using Application.Elements;
using Application.SkillLevels;
using Application.Skills;
using Domain.Enum;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AdminTool.Controllers
{
    [Route("Skills")]
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

        [HttpGet("Create")]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // 선택 옵션들
            var icons = await LoadIconPickListAsync(ct);
            var elements = await client.GetFromJsonAsync<List<ElementDto>>("/api/element", ct) ?? new();

            // 기본 선택값
            var defaultElementId = elements.FirstOrDefault()?.ElementId ?? 0;
            var defaultIconId = icons.FirstOrDefault()?.IconId ?? 0;

            var vm = new SkillCreateVm
            {
                // 드롭다운 기본 선택값
                TypeOptions = BuildSkillTypeOptions(SkillType.Unknown),
                TargetingTypeOptions = BuildSkillTargetOptions(SkillTargetingType.None),
                TargetSideOptions = BuildSkillSideOptions(TargetSideType.None),
                AoeShapeOptions = BuildAoeShapeType(AoeShapeType.None),

                // 셀렉트리스트(Selected 반영)
                ElementOptions = elements
                    .Select(e => new SelectListItem(e.Label, e.ElementId.ToString(), e.ElementId == defaultElementId))
                    .ToList(),

                Icons = icons,

                // 모델 기본값
                Name = "",
                Type = SkillType.Unknown,
                ElementId = defaultElementId,
                IconId = defaultIconId,
                IsActive = true,
                TargetingType = SkillTargetingType.None,
                TargetSide = TargetSideType.None,
                AoeShape = AoeShapeType.None,
                Tag = Array.Empty<string>(),
                Etc = null,           // 뷰에서 Etc를 사용한다면 여기와 POST 매핑을 일치
            };

            // (아이콘 프리뷰를 쓰면) 초기 프리뷰 용도
            ViewBag.SelectedIconUrl = icons.FirstOrDefault(i => i.IconId == vm.IconId)?.Url;

            return View(vm);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SkillCreateVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                // 실패 시 옵션 재로딩
                vm.TypeOptions = BuildSkillTypeOptions(vm.Type);
                vm.ElementOptions = await BuildElementOptionsAsync(vm.ElementId, ct);
                vm.Icons = await LoadIconPickListAsync(ct, vm.IconId);
                vm.TargetingTypeOptions = BuildSkillTargetOptions(vm.TargetingType);
                vm.AoeShapeOptions = BuildAoeShapeType(vm.AoeShape);
                vm.TargetSideOptions = BuildSkillSideOptions(vm.TargetSide);
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");

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
                // 뷰가 Etc 문자열을 쓰면 API에 맞춰 JsonObject로 변환
                BaseInfo = string.IsNullOrWhiteSpace(vm.Etc)
                                ? null
                                : new JsonObject { ["etc"] = vm.Etc }
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

            var created = await resp.Content.ReadFromJsonAsync<SkillDto>(cancellationToken: ct);

            TempData["Message"] = "스킬이 생성되었습니다. 레벨을 추가하세요.";
            // 기존: return RedirectToAction(nameof(Create), new { id = created!.SkillId, tab = "levels" });
            // 변경: 레벨 전용 페이지로
            return RedirectToAction("Levels", new { id = created!.SkillId });
            // 또는: return RedirectToAction("Levels", new { id = created!.SkillId });
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
        #region 스킬 레벨 관련 
        [HttpGet("{id:int}/Levels", Name = "Skills_Levels")]
        public async Task<IActionResult> Levels(int id, CancellationToken ct)
        {
            Console.WriteLine("[SkillsLevels] - Get Levels");
            var client = _http.CreateClient("GameApi");

            // 스킬에 대한 정보.
            var skill = await client.GetFromJsonAsync<SkillDto>($"/api/skills/{id}", ct);
            if (skill is null) return NotFound();

            var items = await client.GetFromJsonAsync<List<SkillLevelDto>>($"/api/skills/{id}/levels", ct)
                        ?? new();

            // AJAX 요청이면 부분뷰만 반환(목록 갱신용)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var listVm = new SkillLevelsVm { SkillId = id, Items = items };
                return PartialView("~/Views/Skills/Partials/_LevelsList.cshtml", listVm);
            }

            // 일반 요청이면 전체 페이지 반환
            var pageVm = new SkillLevelsPageVm
            {
                SkillId = id,
                SkillName = skill.Name,
                ParentType = skill.Type,
                IsPassive = !skill.IsActive,
                Items = items,
                Modal = new LevelEditModalVm("levelEditModal", id, "levelsHost"),
            };

            return View("SkillLevels", pageVm);
        }
        [HttpGet("{id:int}/Levels/New", Name = "Skills_NewLevel")]
        public async Task<IActionResult> NewLevel(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            var s = await client.GetFromJsonAsync<SkillDto>($"/api/skills/{id}", ct);
            if (s is null) return NotFound("Skill not found");

            // 다음 레벨 번호 계산 (없으면 1)
            var levels = await client.GetFromJsonAsync<IReadOnlyList<SkillLevelDto>>($"/api/skills/{id}/levels", ct)
                         ?? Array.Empty<SkillLevelDto>();
            var next = levels.Any() ? levels.Max(x => x.Level) + 1 : 1;

            var vm = new SkillLevelFormVm
            {
                SkillId = id,
                Level = next,
                Values = "",
                Materials = "",
                CostGold = 0,
                ParentType = s.Type,
                IsPassive = !s.IsActive
            };
            return PartialView("Partials/_EditLevelForm", vm);
        }
        [HttpGet("{id:int}/Levels/{level:int}", Name = "Skills_GetLevel")]
        public async Task<IActionResult> GetLevel(int id, int level, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // 편집할 레벨
            var dto = await client.GetFromJsonAsync<SkillLevelDto>($"/api/skills/{id}/levels/{level}", ct);
            if (dto is null) return NotFound();

            // ★ 부모 스킬 정보도 읽어서 VM에 넣기
            var s = await client.GetFromJsonAsync<SkillDto>($"/api/skills/{id}", ct);

            var vm = new SkillLevelFormVm
            {
                SkillId = dto.SkillId,
                Level = dto.Level,
                Values = dto.Values is null ? "{}" : JsonSerializer.Serialize(dto.Values),
                Description = dto.Description,
                Materials = dto.Materials is null ? "{}" : JsonSerializer.Serialize(dto.Materials),
                CostGold = dto.CostGold,
                IsEdit = true,

                // ★ 이 두 줄이 핵심
                ParentType = s?.Type ?? SkillType.Unknown,
                IsPassive = s is null ? false : !s.IsActive
            };

            return PartialView("~/Views/Skills/Partials/_EditLevelForm.cshtml", vm);
        }
        [HttpPost("{id:int}/Levels", Name = "Skills_CreateLevel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLevel(int id, SkillLevelFormVm vm, CancellationToken ct)
        {
            // Values/Materials는 JSON 문자열로 들어옴
            // Values: 자유형 → Dictionary<string, object?> (값은 JsonElement로 들어와도 OK)
            Dictionary<string, object?>? values = null;
            if (!string.IsNullOrWhiteSpace(vm.Values))
                values = JsonSerializer.Deserialize<Dictionary<string, object?>>(vm.Values);

            // Materials: {"501":3} 형태 → Dictionary<string,int>
            Dictionary<string, int>? materials = null;
            if (!string.IsNullOrWhiteSpace(vm.Materials))
                materials = JsonSerializer.Deserialize<Dictionary<string, int>>(vm.Materials);

            var client = _http.CreateClient("GameApi");
            var req = new CreateSkillLevelRequest
            {
                SkillId = id,
                Level = vm.Level,
                Description = vm.Description,
                Values = values,
                Materials = materials,
                CostGold = vm.CostGold
            };

            var resp = await client.PostAsJsonAsync($"/api/skills/{id}/levels", req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode, body);
            return Ok();
        }
        [HttpPost("{id:int}/Levels/{level:int}", Name = "Skills_UpdateLevel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLevel(int id, int level, SkillLevelFormVm vm, CancellationToken ct)
        {
            Dictionary<string, object?>? values = null;
            if (!string.IsNullOrWhiteSpace(vm.Values))
                values = JsonSerializer.Deserialize<Dictionary<string, object?>>(vm.Values);

            // Materials: {"501":3} 형태 → Dictionary<string,int>
            Dictionary<string, int>? materials = null;
            if (!string.IsNullOrWhiteSpace(vm.Materials))
                materials = JsonSerializer.Deserialize<Dictionary<string, int>>(vm.Materials);

            var client = _http.CreateClient("GameApi");
            var req = new UpdateSkillLevelRequest
            {
                Description = vm.Description,
                Values = values,
                Materials = materials,
                CostGold = vm.CostGold
            };

            var resp = await client.PutAsJsonAsync($"/api/skills/{id}/levels/{level}", req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode, body);
            return Ok();
        }
        [HttpPost("{id:int}/Levels/{level:int}/Delete", Name = "Skills_DeleteLevel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLevel(int id, int level, CancellationToken ct)
        {
            Console.WriteLine($"[SkillLevel] Delete id : {id} level : {level}");
            var client = _http.CreateClient("GameApi");

            var resp = await client.DeleteAsync($"/api/skills/{id}/levels/{level}", ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode, body);

            // AJAX 호출이면 200만 주고, 클라이언트에서 reloadLevels() 호출
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok();

            // 일반 접근시 목록으로
            return RedirectToRoute("Skills_Levels", new { id });
        }
        #endregion

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
