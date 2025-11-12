using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Monsters
{
    public interface IMonsterCache
    {
        IReadOnlyList<MonsterDto> GetAll();
        MonsterDto? GetById(int id);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
