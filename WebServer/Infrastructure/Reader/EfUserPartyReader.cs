using Application.UserParties;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Reader
{
    public sealed class EfUserPartyReader : IUserPartyReader
    {
        private readonly GameDBContext _db;

        public EfUserPartyReader(GameDBContext db)
        {
            _db = db;
        }

        public async Task<UserPartyDto?> GetAsync(long partyId, CancellationToken ct)
        {  
            var party = await _db.UserParties
                .AsNoTracking()
                .Where(p => p.PartyId == partyId)
                .Select(p => new UserPartyDto
                {
                    PartyId = p.PartyId,
                    UserId = p.UserId,
                    BattleId = p.BattleId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Slots = p.Slots
                        .OrderBy(s => s.SlotId)
                        .Select(s => new PartySlotDto
                        {
                            SlotId = s.SlotId,
                            UserCharacterId = s.UserCharacterId
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);

            return party;
        }
        public async Task<UserPartyDto?> GetByUserBattleAsync(int userId, int battleId, CancellationToken ct)
        {
            var party = await _db.UserParties
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.BattleId == battleId)
                .Select(p => new UserPartyDto
                {
                    PartyId = p.PartyId,
                    UserId = p.UserId,
                    BattleId = p.BattleId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Slots = p.Slots
                        .OrderBy(s => s.SlotId)
                        .Select(s => new PartySlotDto
                        {
                            SlotId = s.SlotId,
                            UserCharacterId = s.UserCharacterId
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);

            return party;
        }
    }
}
