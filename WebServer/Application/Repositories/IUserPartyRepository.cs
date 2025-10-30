using Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IUserPartyRepository
    {
        Task<UserParty?> GetByIdAsync(long partyId, CancellationToken ct = default);
        Task<UserParty?> GetByUserBattleAsync(int userId, int battleId, CancellationToken ct = default);

        /// <summary>헤더 + 빈 슬롯(slotCount개) 생성 후 PartyId 반환</summary>
        Task<long> CreateAsync(int userId, int battleId, int slotCount, CancellationToken ct = default);

        /// <summary>Aggregate의 현재 상태를 저장(Assign/Unassign/Swap 호출 후 사용)</summary>
        Task SaveAsync(UserParty party, CancellationToken ct = default);

        Task<bool> ExistsAsync(int userId, int battleId, CancellationToken ct = default);
    }
}
