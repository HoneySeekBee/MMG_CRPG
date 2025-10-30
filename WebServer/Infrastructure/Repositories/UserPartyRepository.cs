using Application.Repositories;
using Domain.Entities.User;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class UserPartyRepository : IUserPartyRepository
    {
        private readonly GameDBContext _db;

        public UserPartyRepository(GameDBContext db) => _db = db;

        public async Task<UserParty?> GetByIdAsync(long partyId, CancellationToken ct = default)
        {
            return await _db.Set<UserParty>()
                .Include(p => p.Slots)
                .FirstOrDefaultAsync(p => p.PartyId == partyId, ct);
        }

        // 유저+배틀로 조회 (프리셋/다중파티가 없다면 단건)
        public async Task<UserParty?> GetByUserBattleAsync(int userId, int battleId, CancellationToken ct = default)
        {
            return await _db.Set<UserParty>()
                .Include(p => p.Slots)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.BattleId == battleId, ct);
        }
         
        public async Task<long> CreateAsync(int userId, int battleId, int slotCount, CancellationToken ct = default)
        {
            if (slotCount <= 0) throw new ArgumentOutOfRangeException(nameof(slotCount));

            // 헤더만 먼저 저장해 party_id 확보 
            var header = new UserPartyHeaderProxy(userId, battleId);
            var entry = await _db.Set<UserParty>().AddAsync(header, ct);
            await _db.SaveChangesAsync(ct); // party_id 생성

            var partyId = header.PartyId;

            // 슬롯 시드 (user_character_id = NULL)
            var slots = Enumerable.Range(0, slotCount)
                                  .Select(i => new UserPartySlot(partyId, i, null));
            await _db.Set<UserPartySlot>().AddRangeAsync(slots, ct);
            await _db.SaveChangesAsync(ct);

            return partyId;
        }
         
        public async Task SaveAsync(UserParty party, CancellationToken ct = default)
        { 
            _db.Attach(party);
            _db.Entry(party).Property(x => x.UpdatedAt).IsModified = true;
             
            foreach (var slot in party.Slots)
            {
                _db.Attach(slot);
                _db.Entry(slot).Property(x => x.UserCharacterId).IsModified = true;
                 
                _db.Entry(slot).Property("updated_at").CurrentValue = DateTime.UtcNow;
                _db.Entry(slot).Property("updated_at").IsModified = true;
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> ExistsAsync(int userId, int battleId, CancellationToken ct = default)
        {
            return await _db.Set<UserParty>()
                .AnyAsync(p => p.UserId == userId && p.BattleId == battleId, ct);
        }
          
        private sealed class UserPartyHeaderProxy : UserParty
        {
            public UserPartyHeaderProxy(int userId, int battleId)
            {
                typeof(UserParty).GetProperty(nameof(UserParty.UserId))!
                    .SetValue(this, userId);
                typeof(UserParty).GetProperty(nameof(UserParty.BattleId))!
                    .SetValue(this, battleId);
                typeof(UserParty).GetProperty(nameof(UserParty.CreatedAt))!
                    .SetValue(this, DateTime.UtcNow);
                typeof(UserParty).GetProperty(nameof(UserParty.UpdatedAt))!
                    .SetValue(this, DateTime.UtcNow);
            }

            // PartyId 읽기 (EF가 SaveChanges 시 채워줌)
            public long PartyId
            {
                get => (long)typeof(UserParty).GetProperty(nameof(UserParty.PartyId))!.GetValue(this)!;
                set => typeof(UserParty).GetProperty(nameof(UserParty.PartyId))!.SetValue(this, value);
            }
        }

    }
}
