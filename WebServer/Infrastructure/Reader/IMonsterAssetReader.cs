using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Protos.Monsters;

namespace Infrastructure.Reader
{
    public interface IMonsterAssetReader
    {
        Task<IReadOnlyDictionary<int, MonsterPb>> GetMonstersAsync(
            IReadOnlyCollection<int> ids, CancellationToken ct);
    }

}
