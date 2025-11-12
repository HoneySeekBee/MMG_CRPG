using Contracts.UserParty;
using Domain.Entities.User;
using Google.Protobuf.WellKnownTypes;

namespace WebServer.Mappers
{
    public static class UserPartyProtoMapper
    {
        public static UserPartyPb ToProto(this UserParty party)
        {
            var pb = new UserPartyPb
            {
                PartyId = party.PartyId,
                UserId = party.UserId,
                BattleId = party.BattleId
            };


            pb.Slots.AddRange(party.Slots
                .OrderBy(s => s.SlotId)
                .Select(s => new UserPartySlotPb
                {
                    SlotId = s.SlotId, 
                    UserCharacterId = s.UserCharacterId 
                }));

            return pb;
        }

        public static GetUserPartyResponsePb ToGetResponse(this UserParty party)
            => new() { Party = party.ToProto() };

        public static UserParty ToEntity(this UserPartyPb pb)
        {
            var entity = UserParty.Create(pb.PartyId, pb.UserId, pb.BattleId, pb.Slots.Count);

            foreach (var slot in pb.Slots)
            {
                // Int32Value → int? 변환
                int? userCharacterId = slot.UserCharacterId;
                if (userCharacterId.HasValue)
                    entity.Assign(slot.SlotId, userCharacterId.Value);
            }

            return entity;
        }
    }
}
