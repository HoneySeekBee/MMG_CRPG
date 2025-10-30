using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserParties
{
    public sealed class CreateUserPartyRequest
    {
        [Range(1, int.MaxValue)] public int UserId { get; init; }
        [Range(1, int.MaxValue)] public int BattleId { get; init; }
        [Range(1, 100)] public int SlotCount { get; init; } = 5;
    }
    public sealed class GetUserPartyByUserBattleRequest
    {
        [Range(1, int.MaxValue)] public int UserId { get; init; }
        [Range(1, int.MaxValue)] public int BattleId { get; init; }
    }
    public sealed class GetUserPartyByIdRequest
    {
        [Range(1, long.MaxValue)] public long PartyId { get; init; }
    }

    // 등록 
    public sealed class AssignCharacterRequest
    {
        [Range(1, long.MaxValue)] public long PartyId { get; init; }
        public int SlotId { get; init; }
        [Range(1, int.MaxValue)] public int UserCharacterId { get; init; }
    }

    // 해제 
    public sealed class UnassignCharacterRequest
    {
        [Range(1, long.MaxValue)] public long PartyId { get; init; }
        public int SlotId { get; init; }
    }

    // 스왑
    public sealed class SwapSlotsRequest
    {
        [Range(1, long.MaxValue)] public long PartyId { get; init; }
        public int SlotA { get; init; }
        public int SlotB { get; init; }
    }
}
