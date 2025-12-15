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
using AdminTool.Services;
using AdminTool.Helper;

namespace AdminTool.Controllers
{
    public class IconsController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IAdminAssetUrlBuilder _assetUrl;
        private readonly IAdminAssetCatalog _catalog;

        public IconsController(IHttpClientFactory http, IConfiguration cfg, IAdminAssetUrlBuilder assetUrl, IAdminAssetCatalog catalog)
        {
            _http = http;
            _assetUrl = assetUrl;
            _catalog = catalog;
        }
        public sealed class IconApiDto
        {
            public int IconId { get; set; }
            public string Key { get; set; } = "";
            public int Version { get; set; }
            public string? Url { get; set; }
        }
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var items = await _catalog.GetIconsAsync(ct);
            var model = items.Select(x => new IconVm
            {
                IconId = x.IconId,
                Key = x.Key,
                Version = x.Version,
                Url = _assetUrl.Icon(x.Key, x.Version)
            }).ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IconCreateVm model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.IconFile is null || model.IconFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.IconFile), "이미지 파일을 선택하세요.");
                return View(model);
            }

            var client = _http.CreateClient("GameApi");

            byte[] pngBytes;
            try
            {
                pngBytes = await ImageUploadHelper.ToPngBytesAsync(model.IconFile, ct);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"이미지 처리 중 오류: {ex.Message}";
                return View(model);
            }

            using var form = ImageUploadHelper.BuildUploadForm(model.Key, pngBytes);
            var uploadResp = await client.PostAsync("/api/icons/upload", form, ct);
            if (!uploadResp.IsSuccessStatusCode)
            {
                var body = await uploadResp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"아이콘 업로드 실패: {(int)uploadResp.StatusCode} {uploadResp.ReasonPhrase} - {body}";
                return View(model);
            }

            TempData["Message"] = $"[{model.Key}] 아이콘 업로드 완료";
            _catalog.InvalidateIcons();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            if (id <= 0)
            {
                TempData["Error"] = "잘못된 요청입니다.";
                return RedirectToAction(nameof(Index));
            }

            var client = _http.CreateClient("GameApi");
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
            if (dto is null)
            {
                TempData["Error"] = "아이콘 데이터를 읽을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new IconEditVm
            {
                IconId = dto.IconId,
                Key = dto.Key,
                CurrentVersion = dto.Version,
                ImageUrl = _assetUrl.Icon(dto.Key, dto.Version)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IconEditVm model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.IconFile is null || model.IconFile.Length == 0)
            {
                TempData["Message"] = "변경 사항이 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            var client = _http.CreateClient("GameApi");

            // Key 확보
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
            if (dto is null)
            {
                TempData["Error"] = "아이콘 데이터를 읽을 수 없습니다.";
                return RedirectToAction(nameof(Index));
            }

            byte[] pngBytes;
            try
            {
                pngBytes = await ImageUploadHelper.ToPngBytesAsync(model.IconFile, ct);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"이미지 처리 중 오류: {ex.Message}";
                return View(model);
            }

            using var form = ImageUploadHelper.BuildUploadForm(dto.Key, pngBytes);
            var uploadResp = await client.PostAsync("/api/icons/upload", form, ct);
            if (!uploadResp.IsSuccessStatusCode)
            {
                var body = await uploadResp.Content.ReadAsStringAsync(ct);
                TempData["Error"] = $"아이콘 업로드 실패: {(int)uploadResp.StatusCode} {uploadResp.ReasonPhrase} - {body}";
                return View(model);
            }

            TempData["Message"] = $"[{dto.Key}] 아이콘 업로드 완료";
            _catalog.InvalidateIcons();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = _http.CreateClient("GameApi");
            var resp = await client.DeleteAsync($"/api/icons/{id}", ct);

            if (resp.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.OK)
            {
                TempData["Message"] = $"아이콘(id={id})이 삭제되었습니다.";
                _catalog.InvalidateIcons();
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
