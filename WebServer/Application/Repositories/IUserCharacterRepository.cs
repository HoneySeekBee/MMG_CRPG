using Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserCha = Domain.Entities.User.UserCharacter;

namespace Application.Repositories
{
    public interface IUserCharacterRepository
    {
        Task<UserCha?> GetAsync(int userId, int characterId, CancellationToken ct = default);
        Task AddAsync(UserCha entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<UserCha> entities, CancellationToken ct = default);
        Task<(IReadOnlyList<UserCha> Items, int TotalCount)> GetListAsync(
           int userId, int page, int pageSize, CancellationToken ct = default);

        /// <summary>
        /// 유저가 보유한 캐릭터 ID 목록 조회 (N+1 방지용)
        /// </summary>
        Task<HashSet<int>> GetOwnedCharacterIdsAsync(int userId, IEnumerable<int> characterIds, CancellationToken ct = default);
    }
}
