using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Portraits
{
    public interface IPortraitsCache
    {
        IReadOnlyList<PortraitMetaDto> GetAll();
        PortraitMetaDto? GetById(int id);
        PortraitMetaDto? GetByKey(string key);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
