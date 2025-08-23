using Application.Storage;
namespace Infrastructure.Storage
{
    public sealed class LocalIconStorage : IIconStorage
    {
        private readonly string _root; // wwwroot/icons
        private readonly string _publicBase; // https://domain/icons

        public LocalIconStorage(string webRootPath, string publicBaseUrl)
        {
            _root = Path.Combine(webRootPath, "icons");
            Directory.CreateDirectory(_root);
            _publicBase = publicBaseUrl.TrimEnd('/') + "/icons";
        }

        public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
        {
            var path = Path.Combine(_root, $"{key}.png");
            await using var fs = File.Create(path);
            await content.CopyToAsync(fs, ct);
        }

        public Task DeleteAsync(string key, CancellationToken ct = default)
        {
            var path = Path.Combine(_root, $"{key}.png");
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        => Task.FromResult(File.Exists(Path.Combine(_root, $"{key}.png")));

        public string GetPublicUrl(string key, int version)
        => $"{_publicBase}/{key}.png?v={version}";
    }
}
