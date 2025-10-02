using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Elements
{
    public interface IElementCache
    {
        IReadOnlyList<ElementDto> GetAll();
        ElementDto? GetById(int id);
        ElementDto? GetByKey(string key);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
