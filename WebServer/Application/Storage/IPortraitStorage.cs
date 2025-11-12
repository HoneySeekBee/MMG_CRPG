using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Storage
{
    public interface IPortraitStorage
    {
        Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default);
        Task<bool> ExistsAsync(string key, CancellationToken ct = default);
        Task DeleteAsync(string key, CancellationToken ct = default);
        string GetPublicUrl(string key, int version);
    }
}
