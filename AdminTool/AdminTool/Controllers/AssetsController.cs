using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace AdminTool.Controllers
{
    public class AssetsController : Controller
    {
        private readonly IHttpClientFactory _http;
        public AssetsController(IHttpClientFactory http) => _http = http;

        [HttpGet("/admin/assets/icons/{key}")]
        public async Task<IActionResult> Icon(string key, CancellationToken ct)
            => await ProxyImage($"/api/image/icons/{Uri.EscapeDataString(key)}", ct);

        [HttpGet("/admin/assets/portraits/{key}")]
        public async Task<IActionResult> Portrait(string key, CancellationToken ct)
            => await ProxyImage($"/api/image/portraits/{Uri.EscapeDataString(key)}", ct);

        private async Task<IActionResult> ProxyImage(string path, CancellationToken ct)
        {
            var token = HttpContext.Session.GetString("access_token");
            if (string.IsNullOrEmpty(token))
                return Unauthorized("No access token");

            var client = _http.CreateClient("GameApi");
            using var req = new HttpRequestMessage(HttpMethod.Get, path);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var resp = await client.SendAsync(req, ct);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
                return Unauthorized();

            if (!resp.IsSuccessStatusCode)
                return StatusCode((int)resp.StatusCode);

            var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
            var contentType = resp.Content.Headers.ContentType?.ToString() ?? "image/png";
            return File(bytes, contentType);
        }
    }
}
