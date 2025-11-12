using AdminTool.Models;
using Application.Elements;
using Application.Monsters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdminTool.Controllers
{
    public class MonstersController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;

        public MonstersController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;
        }

        // GET: /Monster
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            var vm = new MonsterIndexVm();

            var resp = await client.GetAsync("/api/monster", ct);
            if (!resp.IsSuccessStatusCode)
            {
                // API가 죽었을 때도 화면은 보여주자
                ViewData["ApiError"] = $"API /api/monster 호출 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}";
                return View(vm);
            }

            // 여기까지 왔으면 OK
            var apiMonsters = await resp.Content.ReadFromJsonAsync<List<MonsterDtoStub>>(cancellationToken: ct)
                              ?? new List<MonsterDtoStub>();
            var ports = await client.GetFromJsonAsync<List<PortraitVm>>("/api/portraits", ct) ?? new();
            var baseUrl = _cfg["PublicBaseUrl"]!.TrimEnd('/');
            var subdir = _cfg["Assets:PortraitsSubdir"] ?? "portraits";
            vm.Monsters = apiMonsters.Select(m =>
            {
                string? portraitUrl = null;
                if (m.PortraitId is not null)
                {
                    var p = ports.FirstOrDefault(x => x.PortraitId == m.PortraitId);
                    if (p != null)
                    {
                        portraitUrl = $"{baseUrl}/{subdir}/{p.Key}.png?v={p.Version}";
                    }
                }

                return new MonsterListItemVm
                {
                    Id = m.Id,
                    Name = m.Name,
                    ModelKey = m.ModelKey,
                    ElementId = m.ElementId,
                    PortraitId = m.PortraitId,
                    StatCount = m.Stats?.Count ?? 0,
                    PortraitUrl = portraitUrl 
                };
            }).ToList();
            return View(vm);
        }

        // GET: /Monster/Create
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var vm = new MonsterEditVm();
            await FillPortraitsAsync(vm, ct);
            await FillElementsAsync(vm, ct);
            return View(vm);
        }

        // POST: /Monster/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonsterEditVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await FillPortraitsAsync(vm, ct);
                await FillElementsAsync(vm, ct);
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");

            var req = new
            {
                name = vm.Name,
                modelKey = vm.ModelKey,
                elementId = vm.ElementId,
                portraitId = vm.PortraitId,
                stats = Array.Empty<object>() // 처음엔 비워둠
            };

            var resp = await client.PostAsJsonAsync("/api/monster", req, ct);
            resp.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Monster/Edit/5
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var apiMonster = await client.GetFromJsonAsync<MonsterDtoStub>($"/api/monster/{id}", ct);

            if (apiMonster is null)
                return NotFound();

            var vm = new MonsterEditVm
            {
                Id = id,
                Name = apiMonster.Name,
                ModelKey = apiMonster.ModelKey,
                ElementId = apiMonster.ElementId,
                PortraitId = apiMonster.PortraitId
            };

            await FillPortraitsAsync(vm, ct);
            await FillElementsAsync(vm, ct);
            return View(vm);
        }
        // POST: /Monster/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MonsterEditVm vm, CancellationToken ct)
        {
            if (id != vm.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await FillPortraitsAsync(vm, ct);
                await FillElementsAsync(vm, ct);
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");

            var req = new
            {
                id = vm.Id,
                name = vm.Name,
                modelKey = vm.ModelKey,
                elementId = vm.ElementId,
                portraitId = vm.PortraitId
            };

            var resp = await client.PutAsJsonAsync($"/api/monster/{id}", req, ct);
            resp.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Detail(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // 1) 몬스터 기본 정보 로드 (API에는 PortraitUrl이 보통 없음)
            var dto = await client.GetFromJsonAsync<MonsterDtoStub>($"/api/monster/{id}", ct);
            if (dto is null) return NotFound();

            // 2) 초상화 목록을 불러와 URL 계산
            string? portraitUrl = null;
            if (dto.PortraitId is not null)
            {
                var ports = await client.GetFromJsonAsync<List<PortraitVm>>("/api/portraits", ct) ?? new();
                var p = ports.FirstOrDefault(x => x.PortraitId == dto.PortraitId);
                if (p != null)
                {
                    var baseUrl = _cfg["PublicBaseUrl"]!.TrimEnd('/');
                    var subdir = _cfg["Assets:PortraitsSubdir"] ?? "portraits";
                    portraitUrl = $"{baseUrl}/{subdir}/{p.Key}.png?v={p.Version}";
                }
            }

            // 3) 뷰모델로 매핑
            var vm = new MonsterDetailVm
            {
                Id = dto.Id,
                Name = dto.Name,
                ModelKey = dto.ModelKey,
                ElementId = dto.ElementId,
                PortraitUrl = portraitUrl,
                Stats = (dto.Stats ?? new()).Select(s => new MonsterStatVm
                {
                    MonsterId = dto.Id,
                    Level = s.Level,
                    HP = s.HP,
                    ATK = s.ATK,
                    DEF = s.DEF,
                    SPD = s.SPD,
                    CritRate = s.CritRate,
                    CritDamage = s.CritDamage
                }).OrderBy(s => s.Level).ToList()
            };

            return View(vm);
        }
        // GET: /Monsters/StatCreate?monsterId=4
        [HttpGet]
        public IActionResult StatCreate(int monsterId)
        {
            // 기본값 채워서 폼 오픈
            var vm = new MonsterStatVm
            {
                MonsterId = monsterId,
                CritRate = 5.00m,
                CritDamage = 150.00m
            };
            return View(vm); // Views/Monsters/StatCreate.cshtml
        }

        // POST: /Monsters/StatCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StatCreate(MonsterStatVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var client = _http.CreateClient("GameApi");

            var req = new
            {
                monsterId = vm.MonsterId,
                level = vm.Level,
                hp = vm.HP,
                atk = vm.ATK,
                def = vm.DEF,
                spd = vm.SPD,
                critRate = vm.CritRate,
                critDamage = vm.CritDamage
            };

            var resp = await client.PostAsJsonAsync("/api/monster/stat", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty,
                    $"API 호출 실패: {(int)resp.StatusCode} {resp.ReasonPhrase}");
                return View(vm);
            }

            return RedirectToAction(nameof(Detail), new { id = vm.MonsterId });
        }
        // GET: /Monsters/StatEdit?monsterId=4&level=1
        public async Task<IActionResult> StatEdit(int monsterId, int level, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // 몬스터 정보 로드
            var dto = await client.GetFromJsonAsync<MonsterDtoStub>($"/api/monster/{monsterId}", ct);
            if (dto is null) return NotFound();

            // 대상 스탯 찾기
            var stat = (dto.Stats as List<MonsterStatDto> ?? new())
                .FirstOrDefault(s => s.Level == level);

            if (stat is null) return NotFound();

            var vm = new MonsterStatVm
            {
                MonsterId = monsterId,
                Level = stat.Level,
                HP = stat.HP,
                ATK = stat.ATK,
                DEF = stat.DEF,
                SPD = stat.SPD,
                CritRate = stat.CritRate,
                CritDamage = stat.CritDamage
            };

            return View(vm);
        }

        // POST: /Monsters/StatEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StatEdit(MonsterStatVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var client = _http.CreateClient("GameApi");

            // API 구조 예시: PUT /api/monster/{id}/stat/{level}
            var req = new
            {
                monsterId = vm.MonsterId,
                level = vm.Level,
                hp = vm.HP,
                atk = vm.ATK,
                def = vm.DEF,
                spd = vm.SPD,
                critRate = vm.CritRate,
                critDamage = vm.CritDamage
            };
            var resp = await client.PostAsJsonAsync("/api/monster/stat", req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                ModelState.AddModelError(string.Empty, $"API 호출 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} / {body}");
                return View(vm);
            }

            return RedirectToAction(nameof(Detail), new { id = vm.MonsterId });
        }
        [HttpGet]
        public IActionResult StatsBulk() => View(new MonsterStatsBulkVm());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StatsBulk(MonsterStatsBulkVm vm, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(vm.RawTable))
            {
                ModelState.AddModelError(nameof(vm.RawTable), "내용을 붙여넣어 주세요.");
                return View(vm);
            }

            var client = _http.CreateClient("GameApi");

            // 1) 몬스터 목록/포트레이트 등 미리 로드 (이름/모델키 → Id 매핑)
            var all = await client.GetFromJsonAsync<List<MonsterDtoStub>>("/api/monster", ct) ?? new();

            // 2) 라인 파싱
            var lines = vm.RawTable
                .Replace("\r\n", "\n")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            int ok = 0, fail = 0;
            var errors = new List<string>();

            foreach (var line in lines)
            {
                // 탭/쉼표 모두 지원 (우선 탭 → 엑셀복붙), 탭이 없으면 쉼표 split
                var cols = line.Contains('\t')
                    ? line.Split('\t')
                    : line.Split(',');

                if (cols.Length < 9)
                {
                    // 헤더면 넘어감
                    if (line.Contains("레벨") || line.Contains("Level", StringComparison.OrdinalIgnoreCase)) continue;
                    fail++; errors.Add($"열 개수 부족: {line}");
                    continue;
                }

                var name = cols[0].Trim();
                var modelKey = cols[1].Trim();
                int level = 0, hp = 0, atk = 0, def_ = 0, spd = 0;
                bool okLevel = int.TryParse(cols[2].Trim(), out level);
                bool okHp = int.TryParse(cols[3].Trim(), out hp);
                bool okAtk = int.TryParse(cols[4].Trim(), out atk);
                bool okDef = int.TryParse(cols[5].Trim(), out def_);
                bool okSpd = int.TryParse(cols[6].Trim(), out spd);
                bool parsed = okLevel && okHp && okAtk && okDef && okSpd;
                if (!parsed)
                {
                    fail++; errors.Add($"정수 파싱 실패: {line}");
                    continue;
                }
                decimal ParsePercent(string s)
                {
                    s = s.Trim().Replace("%", ""); 
                    return decimal.TryParse(
                        s,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var d
                    ) ? d : 0m;
                }

                var critRate = ParsePercent(cols[7]);
                var critDamage = ParsePercent(cols[8]);
                // 3) 대상 몬스터 찾기 (모델키 우선, 없으면 이름)
                var monster = all.FirstOrDefault(m =>
                                    string.Equals(m.ModelKey, modelKey, StringComparison.OrdinalIgnoreCase))
                           ?? all.FirstOrDefault(m =>
                                    string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));

                if (monster is null)
                {
                    fail++; errors.Add($"몬스터를 찾을 수 없음: name={name}, modelKey={modelKey}");
                    continue;
                }

                // 4) 업서트 API 호출 (Create/Edit에서 쓰던 엔드포인트)
                var req = new
                {
                    monsterId = monster.Id,
                    level = level,
                    hp = hp,
                    atk = atk,
                    def = def_,
                    spd = spd,
                    critRate = critRate,
                    critDamage = critDamage
                }; 
                var resp = await client.PostAsJsonAsync("/api/monster/stat", req, ct);
                if (resp.IsSuccessStatusCode) ok++;
                else
                {
                    var body = await resp.Content.ReadAsStringAsync(ct);
                    fail++; errors.Add($"업서트 실패: {monster.Id}/{level} ({(int)resp.StatusCode}) {body}");
                }
            }

            TempData["Flash"] = $"업로드 완료: OK={ok}, FAIL={fail}";
            if (errors.Count > 0) TempData["FlashDetail"] = string.Join("\n", errors.Take(20));
            return RedirectToAction(nameof(Index));
        }

        // POST: /Monster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/monster/{id}", ct);
            resp.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }
        private async Task FillPortraitsAsync(MonsterEditVm vm, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var apiPorts = await client.GetFromJsonAsync<List<PortraitVm>>("/api/portraits", ct) ?? new();

            var baseUrl = _cfg["PublicBaseUrl"]!.TrimEnd('/');
            var subdir = _cfg["Assets:PortraitsSubdir"] ?? "portraits";

            vm.PortraitChoices = apiPorts
                .Select(p => new PortraitPickItem
                {
                    PortraitId = p.PortraitId,
                    Key = p.Key,
                    Version = p.Version,
                    Url = $"{baseUrl}/{subdir}/{p.Key}.png?v={p.Version}"
                })
                .ToList();

            if (vm.PortraitId.HasValue)
            {
                vm.SelectedPortraitUrl = vm.PortraitChoices
                    .FirstOrDefault(x => x.PortraitId == vm.PortraitId.Value)
                    ?.Url;
            }
        }
        private async Task FillElementsAsync(MonsterEditVm vm, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");

            // 라우트가 단수/복수일 수 있으니 둘 다 시도
            var elems =
                await TryGet<List<ElementDto>>(client, "/api/element", ct)
                ?? await TryGet<List<ElementDto>>(client, "/api/elements", ct)
                ?? new List<ElementDto>();

            vm.Elements = elems
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.ElementId)
                .Select(e => new SelectListItem(
                    text: e.Label,
                    value: e.ElementId.ToString(),
                    selected: e.ElementId == vm.ElementId
                ))
                .ToList();
        }
        private static async Task<T?> TryGet<T>(HttpClient client, string url, CancellationToken ct)
        {
            try
            {
                var resp = await client.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode)
                    return default;

                return await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
            }
            catch
            {
                return default;
            }
        }

        // 이 컨트롤러 안에서만 쓸 간단한 DTO
        private sealed class ElementDto
        {
            public int ElementId { get; set; }
            public string Label { get; set; } = string.Empty;
            public int SortOrder { get; set; }
        }
    }
} 