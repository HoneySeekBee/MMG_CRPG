using Application.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Storage
{
    public sealed class LocalPortraitStorage : IPortraitStorage
    {
        private readonly string _root;        // e.g., {webRootPath}/portraits
        private readonly string _publicBase;  // e.g., https://host/portraits

        public LocalPortraitStorage(string webRootPath, string publicBaseUrl)
        {
            _root = Path.Combine(webRootPath, "portraits");
            Directory.CreateDirectory(_root);
            _publicBase = publicBaseUrl.TrimEnd('/') + "/portraits";
        }

        public async Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default)
        {
            var path = Path.Combine(_root, $"{key}.png");
            await using var fs = File.Create(path);
            await content.CopyToAsync(fs, ct);
        }

        public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
            => Task.FromResult(File.Exists(Path.Combine(_root, $"{key}.png")));

        public Task DeleteAsync(string key, CancellationToken ct = default)
        {
            var path = Path.Combine(_root, $"{key}.png");
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }

        public string GetPublicUrl(string key, int version)
            => $"{_publicBase}/{key}.png?v={version}";
    }
}
