using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Rarities
{
    public interface IRarityCache
    {
        IReadOnlyList<RarityDto> GetAll();
        RarityDto? GetById(int id);
        RarityDto? GetByKey(string key);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
