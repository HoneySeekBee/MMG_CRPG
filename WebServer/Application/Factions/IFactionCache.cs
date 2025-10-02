using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Factions
{
    public interface IFactionCache
    {
        IReadOnlyList<FactionDto> GetAll();
        FactionDto? GetById(int id);
        FactionDto? GetByKey(string key);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
