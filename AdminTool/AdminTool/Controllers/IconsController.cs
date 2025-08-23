using AdminTool.Models;
using Microsoft.AspNetCore.Mvc;
using Application.Repositories;   // IIconRepository
using Application.Storage;

namespace AdminTool.Controllers
{
    public class IconsController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly string _assetsBaseUrl;


        public IconsController(IHttpClientFactory http, IConfiguration cfg)
        {
            _http = http;
            _assetsBaseUrl = cfg["PublicBaseUrl"]!.TrimEnd('/');
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
        // (2) 

        // [3] Edit
    }
}
