using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserParties
{
    public sealed class PartySlotDto
    {
        public int SlotId { get; init; }
        public int? UserCharacterId { get; init; } 
    }
    public sealed class UserPartyDto
    {
        public long PartyId { get; init; }
        public int UserId { get; init; }
        public int BattleId { get; init; }
        public IReadOnlyList<PartySlotDto> Slots { get; init; } = Array.Empty<PartySlotDto>();
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
