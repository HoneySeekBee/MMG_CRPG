using Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserParties
{
    public static class PartyMappers
    {
        public static PartySlotDto ToDto(this UserPartySlot slot) =>
        new()
        {
            SlotId = slot.SlotId,
            UserCharacterId = slot.UserCharacterId
        };

        public static UserPartyDto ToDto(this UserParty party) =>
            new()
            {
                PartyId = party.PartyId,
                UserId = party.UserId,
                BattleId = party.BattleId,
                Slots = party.Slots.OrderBy(s => s.SlotId).Select(ToDto).ToList(),
                CreatedAt = party.CreatedAt,
                UpdatedAt = party.UpdatedAt
            };
    }
}
