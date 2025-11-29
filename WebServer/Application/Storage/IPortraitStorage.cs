using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Storage
{
    public interface IPortraitStorage
    {
        string GetPublicUrl(string key, int version);
        Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct);
        Task<byte[]> LoadAsync(string key, CancellationToken ct);
        Task DeleteAsync(string key, CancellationToken ct);
    }
}
