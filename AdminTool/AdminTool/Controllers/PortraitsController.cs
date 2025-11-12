using AdminTool.Models;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System.Net;

namespace AdminTool.Controllers
{
    public class PortraitsController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly string _assetsBaseUrl;

        private readonly string _assetsPhysicalRoot;
        private readonly string _portraitsSubdir;

        public PortraitsController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;

            _assetsBaseUrl = cfg["PublicBaseUrl"]!.TrimEnd('/');

            _assetsPhysicalRoot = cfg["Assets:PhysicalRoot"]!
                ?? throw new InvalidOperationException("Assets:PhysicalRoot 설정이 필요합니다.");

            _portraitsSubdir = cfg["Assets:PortraitsSubdir"] ?? "portraits";
        }

        // API DTO (응답 최소셋만 사용)
        public sealed class PortraitApiDto
        {
            public int PortraitId { get; set; }
            public string Key { get; set; } = "";
            public int Version { get; set; }
            public string? Url { get; set; }
        }

        // ============ [1] Index ============
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var items = await client.GetFromJsonAsync<List<PortraitApiDto>>("api/portraits", ct)
                        ?? new List<PortraitApiDto>();

            var model = items.Select(x => new PortraitVm
            {
                PortraitId = x.PortraitId,
                Key = x.Key,
                Version = x.Version,
                Url = $"{_assetsBaseUrl}/{_portraitsSubdir}/{x.Key}.png?v={x.Version}"
            }).ToList();

            return View(model); // Views/Portraits/Index.cshtml
        }

        // ============ [2] Create ============
        [HttpGet]
        public IActionResult Create() => View(); // Views/Portraits/Create.cshtml

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PortraitCreateVm model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.File is null || model.File.Length == 0)
            {
                ModelState.AddModelError(nameof(model.File), "이미지 파일을 선택하세요.");
                return View(model);
            }

            var client = _http.CreateClient("GameApi");

            // 기존 버전 조회
            var all = await client.GetFromJsonAsync<List<PortraitApiDto>>("api/portraits", ct)
                      ?? new List<PortraitApiDto>();
            var existing = all.FirstOrDefault(p => p.Key == model.Key);
            var newVersion = (existing?.Version ?? 0) + 1;

            // 물리 저장 (wwwroot/portraits/{key}.png)
            var dir = Path.Combine(_assetsPhysicalRoot, _portraitsSubdir);
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, $"{model.Key}.png");

            try
            {
                // SVG 방지(필요 시)
                if (string.Equals(model.File.ContentType, "image/svg+xml", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Path.GetExtension(model.File.FileName), ".svg", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.File), "SVG는 지원하지 않습니다. PNG/JPG/WebP를 업로드하세요.");
                    return View(model);
                }

                using var s = model.File.OpenReadStream();
                using var img = await Image.LoadAsync(s, ct);
                var encoder = new PngEncoder { CompressionLevel = PngCompressionLevel.DefaultCompression };
                await img.SaveAsPngAsync(filePath, encoder, ct);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"이미지 처리 중 오류: {ex.Message}";
                return View(model);
            }

            // 메타 생성/갱신
            if (existing == null)
            {
                var createBody = new { Key = model.Key }; // CreatePortraitCommand와 일치
                var resp = await client.PostAsJsonAsync("api/portraits", createBody, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"초상화 메타 생성 실패: {resp.StatusCode}";
                    return View(model);
                }
                TempData["Message"] = $"[{model.Key}] 초상화가 생성되었습니다. (v1)";
            }
            else
            {
                var updateBody = new { Id = existing.PortraitId, Version = newVersion };
                var resp = await client.PutAsJsonAsync($"api/portraits/{existing.PortraitId}", updateBody, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"초상화 메타 업데이트 실패: {resp.StatusCode}";
                    return View(model);
                }
                TempData["Message"] = $"[{model.Key}] 초상화가 업데이트되었습니다. (v{newVersion})";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============ [3] Edit ============
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                TempData["Error"] = "잘못된 요청입니다.";
                return RedirectToAction(nameof(Index));
            }

            var client = _http.CreateClient("GameApi");
            var resp = await client.GetAsync($"api/portraits/{id}", ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = $"초상화(id={id})을 찾을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"초상화 조회 실패: {resp.StatusCode}";
                return RedirectToAction(nameof(Index));
            }

            var dto = await resp.Content.ReadFromJsonAsync<PortraitApiDto>(cancellationToken: ct);
            if (dto is null)
            {
                TempData["Error"] = "초상화 데이터를 읽을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new PortraitEditVm
            {
                PortraitId = dto.PortraitId,
                Key = dto.Key,
                CurrentVersion = dto.Version,
                ImageUrl = $"{_assetsBaseUrl}/{_portraitsSubdir}/{dto.Key}.png?v={dto.Version}"
            };
            return View(vm); // Views/Portraits/Edit.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PortraitEditVm model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _http.CreateClient("GameApi");

            // 존재 확인
            var resp = await client.GetAsync($"api/portraits/{model.PortraitId}", ct);
            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = $"초상화(id={model.PortraitId})을 찾을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"초상화 조회 실패: {resp.StatusCode}";
                return RedirectToAction(nameof(Index));
            }

            var dto = await resp.Content.ReadFromJsonAsync<PortraitApiDto>(cancellationToken: ct);
            if (dto is null)
            {
                TempData["Error"] = "초상화 데이터를 읽을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            // 새 파일 없으면 변경 없음
            if (model.File is null || model.File.Length == 0)
            {
                TempData["Message"] = "변경 사항이 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            // 저장 경로
            var dir = Path.Combine(_assetsPhysicalRoot, _portraitsSubdir);
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, $"{dto.Key}.png");

            try
            {
                if (string.Equals(model.File.ContentType, "image/svg+xml", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Path.GetExtension(model.File.FileName), ".svg", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.File), "SVG는 지원하지 않습니다. PNG/JPG/WebP를 업로드하세요.");
                    return View(model);
                }

                using var s = model.File.OpenReadStream();
                using var img = await Image.LoadAsync(s, ct);
                var encoder = new PngEncoder { CompressionLevel = PngCompressionLevel.DefaultCompression };
                await img.SaveAsPngAsync(filePath, encoder, ct);

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = $"파일 저장 실패: {filePath}";
                    return View(model);
                }

                // 버전 +1
                var newVersion = dto.Version + 1;
                var updateBody = new { Id = dto.PortraitId, Version = newVersion };
                var updateResp = await client.PutAsJsonAsync($"api/portraits/{dto.PortraitId}", updateBody, ct);
                if (!updateResp.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"초상화 메타 업데이트 실패: {updateResp.StatusCode}";
                    return View(model);
                }

                TempData["Message"] = $"[{dto.Key}] 초상화가 업데이트되었습니다. (v{newVersion})";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"이미지 처리 중 오류: {ex.Message}";
                return View(model);
            }
        }

        // ============ [4] Delete ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"api/portraits/{id}", ct);

            if (resp.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent)
            {
                TempData["Message"] = $"초상화(id={id})가 삭제되었습니다.";
            }
            else if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = $"초상화(id={id})을 찾을 수 없습니다.";
            }
            else
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"삭제 실패: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
