using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserCharacter
{
    public interface IUserCharacterReader
    {
        Task<UserCharacterStatsDto?> GetAsync(long userCharacterId, CancellationToken ct);
        Task<IReadOnlyList<UserCharacterStatsDto>> GetManyAsync(
            IReadOnlyCollection<long> userCharacterIds, CancellationToken ct);
        Task<IReadOnlyList<UserCharacterStatsDto>> GetManyByCharacterIdAsync(
            IReadOnlyCollection<long> characterIds, long userId, CancellationToken ct);
    }
}
