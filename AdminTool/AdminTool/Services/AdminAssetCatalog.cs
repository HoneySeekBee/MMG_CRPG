using AdminTool.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AdminTool.Services
{
    public sealed class AdminAssetCatalog : IAdminAssetCatalog
    {
        private readonly IHttpClientFactory _http;
        private readonly IMemoryCache _cache;

        private const string IconsKey = "assets:icons";
        private const string PortraitsKey = "assets:portraits";

        public AdminAssetCatalog(IHttpClientFactory http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }

        public Task<IReadOnlyList<IconVm>> GetIconsAsync(CancellationToken ct)
            => _cache.GetOrCreateAsync(IconsKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
                entry.SlidingExpiration = TimeSpan.FromMinutes(1);

                var client = _http.CreateClient("GameApi");
                return (IReadOnlyList<IconVm>)(await client.GetFromJsonAsync<List<IconVm>>("/api/icons", ct) ?? new());
            })!;

        public Task<IReadOnlyList<PortraitVm>> GetPortraitsAsync(CancellationToken ct)
            => _cache.GetOrCreateAsync(PortraitsKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
                entry.SlidingExpiration = TimeSpan.FromMinutes(1);

                var client = _http.CreateClient("GameApi");
                return (IReadOnlyList<PortraitVm>)(await client.GetFromJsonAsync<List<PortraitVm>>("/api/portraits", ct) ?? new());
            })!;

        public void InvalidateIcons() => _cache.Remove(IconsKey);
        public void InvalidatePortraits() => _cache.Remove(PortraitsKey);
    }
}
