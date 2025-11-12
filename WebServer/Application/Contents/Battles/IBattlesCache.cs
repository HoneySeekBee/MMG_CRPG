using Domain.Entities.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Battles
{
    public interface IBattlesCache
    {
        IReadOnlyList<BattleDto> GetAll();
        BattleDto? GetById(int id);
        Task ReloadAsync(CancellationToken ct = default);

    }
}
