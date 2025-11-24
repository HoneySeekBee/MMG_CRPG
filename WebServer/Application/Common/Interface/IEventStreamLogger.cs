using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interface
{
    public interface IEventStreamLogger
    {
        Task LogAsync(string stream, Dictionary<string, string> data);
        Task<List<StreamEntryDto>> ReadRecentAsync(string stream, int count, CancellationToken ct = default);

    }
    public record StreamEntryDto(
    string Id,
    Dictionary<string, string> Fields
);
}
