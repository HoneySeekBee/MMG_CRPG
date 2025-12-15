using AdminTool.Services;
using AdminTool.Models;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System.Net;
using AdminTool.Helper;

namespace AdminTool.Controllers
{
    public class PortraitsController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IAdminAssetUrlBuilder _assetUrl;
        private readonly IAdminAssetCatalog _catalog;

        public PortraitsController(IHttpClientFactory http, IConfiguration cfg, IAdminAssetUrlBuilder assetUrl, IAdminAssetCatalog catalog)
        {
            _http = http;
            _assetUrl = assetUrl;
            _catalog = catalog;
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
            try
            {
                var items = await _catalog.GetPortraitsAsync(ct);

                var model = items.Select(x => new PortraitVm
                {
                    PortraitId = x.PortraitId,
                    Key = x.Key,
                    Version = x.Version,
                    Url = _assetUrl.Portrait(x.Key, x.Version)
                }).ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Portraits 로드 실패: {ex.Message}";
                return View(new List<PortraitVm>());
            }
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
            byte[] pngBytes;
            try
            {
                pngBytes = await ImageUploadHelper.ToPngBytesAsync(model.File, ct);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"이미지 처리 중 오류: {ex.Message}";
                return View(model);
            }
            using var form = ImageUploadHelper.BuildUploadForm(model.Key, pngBytes);
            var uploadResp = await client.PostAsync("/api/portraits/upload", form, ct);
            if (!uploadResp.IsSuccessStatusCode)
            {
                var body = await uploadResp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"초상화 업로드 실패: {(int)uploadResp.StatusCode} {uploadResp.ReasonPhrase} - {body}";
                return View(model);
            }

            TempData["Message"] = $"[{model.Key}] 초상화 업로드 완료";
            _catalog.InvalidatePortraits();
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
            var resp = await client.GetAsync($"/api/portraits/{id}", ct);

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
                ImageUrl = _assetUrl.Portrait(dto.Key, dto.Version)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PortraitEditVm model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.File is null || model.File.Length == 0)
            {
                TempData["Message"] = "변경 사항이 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            var client = _http.CreateClient("GameApi");

            // Key 확보
            var resp = await client.GetAsync($"/api/portraits/{model.PortraitId}", ct);
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

            byte[] pngBytes;
            try
            {
                pngBytes = await ImageUploadHelper.ToPngBytesAsync(model.File, ct);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"이미지 처리 중 오류: {ex.Message}";
                return View(model);
            }

            using var form = ImageUploadHelper.BuildUploadForm(dto.Key, pngBytes);
            var uploadResp = await client.PostAsync("/api/portraits/upload", form, ct);
            if (!uploadResp.IsSuccessStatusCode)
            {
                var body = await uploadResp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"초상화 업로드 실패: {(int)uploadResp.StatusCode} {uploadResp.ReasonPhrase} - {body}";
                return View(model);
            }

            TempData["Message"] = $"[{dto.Key}] 초상화 업로드 완료";
            _catalog.InvalidatePortraits();
            return RedirectToAction(nameof(Index));
        }

        // ============ [4] Delete ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/portraits/{id}", ct);

            if (resp.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent)
            {
                TempData["Message"] = $"초상화(id={id})가 삭제되었습니다.";
                _catalog.InvalidatePortraits();
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
