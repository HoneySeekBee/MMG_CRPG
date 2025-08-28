using AdminTool.Models;
using Microsoft.AspNetCore.Mvc;
using Application.Repositories;   // IIconRepository
using Application.Storage;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;                    // GetFromJsonAsync
using SixLabors.ImageSharp;                    // Image
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Net;

namespace AdminTool.Controllers
{
    public class IconsController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly string _assetsBaseUrl;

        private readonly string _assetsPhysicalRoot;
        private readonly string _iconsSubdir;

        public IconsController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;
            _assetsBaseUrl = cfg["PublicBaseUrl"]!.TrimEnd('/');

            _assetsPhysicalRoot = cfg["Assets:PhysicalRoot"]!
                ?? throw new InvalidOperationException("Assets:PhysicalRoot 설정이 필요합니다.");

            _iconsSubdir = cfg["Assets:IconsSubdir"] ?? "icons";
        }

        // 운영툴에서 Icons 이미지를 수정 및 관리한다. 

        // [1] Index
        // Get
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var items = await client.GetFromJsonAsync<List<IconApiDto>>("/api/icons", ct)
                         ?? new List<IconApiDto>();
            var model = items.Select(x => new IconVm
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = $"{_assetsBaseUrl}/icons/{x.Key}.png?v={x.Version}"
            }).ToList();
            return View(model);
        }
        public sealed class IconApiDto
        {
            public int IconId { get; set; }
            public string Key { get; set; } = "";
            public int Version { get; set; }
            public string? Url { get; set; }
        }
        // [2] Create

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IconCreateVm model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.IconFile == null || model.IconFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.IconFile), "이미지 파일을 선택하세요.");
                return View(model);
            }
            var client = _http.CreateClient("GameApi");

            var all = await client.GetFromJsonAsync<List<IconApiDto>>("/api/icons", ct)
                      ?? new List<IconApiDto>();
            var existing = all.FirstOrDefault(i => i.Key == model.Key);
            var newVersion = (existing?.Version ?? 0) + 1;

            // 2) 이미지 PNG로 강제 저장: wwwroot/icons/[Key].png (덮어쓰기)
            var iconsDir = Path.Combine(_assetsPhysicalRoot, _iconsSubdir);
            Directory.CreateDirectory(iconsDir);

            var fileName = $"{model.Key}.png";
            var filePath = Path.Combine(iconsDir, fileName);

            try
            {
                using var stream = model.IconFile.OpenReadStream();
                using var image = await Image.LoadAsync(stream, ct);

                var encoder = new PngEncoder { CompressionLevel = PngCompressionLevel.DefaultCompression };
                await image.SaveAsPngAsync(filePath, encoder, ct);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"이미지 처리 중 오류: {ex.Message}";
                return View(model);
            }

            // 3) GameApi에 메타데이터 반영 (존재하면 업데이트, 없으면 생성)
            if (existing == null)
            {
                var createBody = new { Key = model.Key, Version = 1 };
                var resp = await client.PostAsJsonAsync("/api/icons", createBody, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"아이콘 메타 생성 실패: {resp.StatusCode}";
                    return View(model);
                }

                TempData["Message"] = $"[{model.Key}] 아이콘이 생성되었습니다. (v1)";
            }
            else
            {
                var updateBody = new { Version = newVersion };
                var resp = await client.PutAsJsonAsync($"/api/icons/{existing.IconId}", updateBody, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"아이콘 메타 업데이트 실패: {resp.StatusCode}";
                    return View(model);
                }

                TempData["Message"] = $"[{model.Key}] 아이콘이 업데이트되었습니다. (v{newVersion})";
            }

            TempData["Message"] = $"[{model.Key}] 아이콘이 {(existing == null ? "생성" : "업데이트")}되었습니다. (v{newVersion})";
            return RedirectToAction(nameof(Index));
        }

        // [3] Edit
        // (1) Get id를 기반으로 받아와야한다. 
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                TempData["Error"] = "잘못된 요청입니다.";
                return RedirectToAction(nameof(Index));
            }

            var client = _http.CreateClient("GameApi");

            // 응답 코드를 안전하게 처리
            var resp = await client.GetAsync($"/api/icons/{id}", ct);
            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = $"아이콘(id={id})을 찾을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"아이콘 조회 실패: {resp.StatusCode}";
                return RedirectToAction(nameof(Index));
            }

            var dto = await resp.Content.ReadFromJsonAsync<IconApiDto>(cancellationToken: ct);
            if (dto == null)
            {
                TempData["Error"] = "아이콘 데이터를 읽을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new IconEditVm
            {
                IconId = dto.IconId,
                Key = dto.Key,
                CurrentVersion = dto.Version,
                ImageUrl = $"{_assetsBaseUrl}/icons/{dto.Key}.png?v={dto.Version}"
            };
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IconEditVm model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _http.CreateClient("GameApi");

            Console.WriteLine("1_Edit");
            // [1] 해당 아이디가 있는지 확인
            var resp = await client.GetAsync($"/api/icons/{model.IconId}", ct);
            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = $"아이콘(id={model.IconId})을 찾을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Error"] = $"아이콘 조회 실패: {resp.StatusCode}";
                return RedirectToAction(nameof(Index));
            }

            var dto = await resp.Content.ReadFromJsonAsync<IconApiDto>(cancellationToken: ct);
            if (dto == null)
            {
                TempData["Error"] = "아이콘 데이터를 읽을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine("2_Edit");

            // 2) 새 파일이 없는 경우 → 변경 없음
            if (model.IconFile == null || model.IconFile.Length == 0)
            {
                TempData["Message"] = "변경 사항이 없습니다.";
                return RedirectToAction(nameof(Index));
            }
            Console.WriteLine("3_Edit");
            // 3) 파일 저장 경로 준비 (WebServer의 wwwroot/icons/[Key].png)
            var iconsDir = Path.Combine(_assetsPhysicalRoot, _iconsSubdir); // 예: C:\...\WebServer\wwwroot\icons
            Directory.CreateDirectory(iconsDir);

            var filePath = Path.Combine(iconsDir, $"{dto.Key}.png");

            try
            {
                if (string.Equals(model.IconFile.ContentType, "image/svg+xml", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Path.GetExtension(model.IconFile.FileName), ".svg", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.IconFile), "SVG는 PNG로 변환을 지원하지 않습니다. PNG/JPG/WebP를 업로드하세요.");
                    return View(model);
                }
                Console.WriteLine("4_Edit");

                using var s = model.IconFile.OpenReadStream();
                using var img = await Image.LoadAsync(s, ct); // 어떤 형식이든 로드
                var encoder = new PngEncoder { CompressionLevel = PngCompressionLevel.DefaultCompression };
                await img.SaveAsPngAsync(filePath, encoder, ct);

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = $"파일 저장 실패: {filePath}";
                    return View(model);
                }

                Console.WriteLine("5_Edit");
                // 4) 버전 +1 후 GameApi 메타 업데이트
                var newVersion = dto.Version + 1;
                var updateBody = new { Id = dto.IconId, Version = newVersion }; // ← 반드시 'Id' 이름
                var updateResp = await client.PutAsJsonAsync($"/api/icons/{dto.IconId}", updateBody, ct);
                if (!updateResp.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"아이콘 메타 업데이트 실패: {updateResp.StatusCode}";
                    return View(model);
                }

                TempData["Message"] = $"[{dto.Key}] 아이콘이 업데이트되었습니다. (v{newVersion})";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"이미지 처리 중 오류: {ex.Message}";
                return View(model);
            }
        }

        // [4] Delete 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/icons/{id}", ct);

            if (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.OK)
            {
                TempData["Message"] = $"아이콘(id={id})이 삭제되었습니다.";
            }
            else if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = $"아이콘(id={id})을 찾을 수 없습니다.";
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
