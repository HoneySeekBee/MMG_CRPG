using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Icons
{
    public interface IIconCache
    {
        IReadOnlyList<IconMetaDto> GetAll();
        IconMetaDto? GetById(int id);
        IconMetaDto? GetByKey(string key);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
