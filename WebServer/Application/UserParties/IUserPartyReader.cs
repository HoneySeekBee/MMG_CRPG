using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserParties
{
    public interface IUserPartyReader
    {
        Task<UserPartyDto?> GetAsync(long partyId, CancellationToken ct);
        Task<UserPartyDto?> GetByUserBattleAsync(int userId, int battleId, CancellationToken ct);
    } 
}
