using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{

    // 기본 골자인데 어자피 UserParty를 통으로 받을것이기 때문에 Slot
    public class UserPartySlot
    {
        public long PartyId{ get; set; }
        public int SlotId{ get; set; }
        public int? UserCharacterId { get; private set; }
        private UserPartySlot() { }

        public UserPartySlot(long partyId, int slotId, int? userCharacterId)
        {
            PartyId = partyId;
            SlotId = slotId;
            UserCharacterId = userCharacterId;
        }
        public void SetCharacter(int? userCharacterId)
        {
            UserCharacterId = userCharacterId; 
        }
    }
    public class UserParty
    {
        private readonly List<UserPartySlot> _slots = new();
        
        public long PartyId { get; private set; }
        public int UserId { get; private set; }
        public int BattleId { get; private set; }
        public IReadOnlyList<UserPartySlot> Slots => _slots;

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        protected UserParty() { }

        public UserParty(long partyId, int userId, int battleId, int slotCount)
        {
            if(slotCount <= 0) 
                throw new ArgumentOutOfRangeException(nameof(slotCount));

            PartyId = partyId;
            UserId = userId;
            BattleId = battleId;

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;

            for (var i = 0; i < slotCount; i++)
                _slots.Add(new UserPartySlot(partyId, i, null));
        }
        // 파티 만들기
        public static UserParty Create(long partyId, int userId, int battleId, int slotCount)
            => new(partyId, userId, battleId, slotCount);
        
        // 캐릭터 파티 참가
        public void Assign(int slotId, int userCharacterId)
        {
            EnsureSlotCapacity(slotId);
            if (_slots.Any(s => s.UserCharacterId == userCharacterId && s.SlotId != slotId))
                throw new InvalidOperationException($"Character {userCharacterId} already assigned in this party.");

            var slot = GetSlotById(slotId);
            slot.SetCharacter(userCharacterId);
            Touch();
        }
        // 캐릭터 해제 
        public void Unassign(int slotId)
        {
            EnsureSlotCapacity(slotId);
            var slot = GetSlotById(slotId);
            slot.SetCharacter(null);
            Touch();
        }
        // 교체 
        public void Swap(int slotA, int slotB)
        {
            EnsureSlotCapacity(slotA);
            EnsureSlotCapacity(slotB);
            if (slotA == slotB) return;

            var a = _slots[slotA].UserCharacterId;
            var b = _slots[slotB].UserCharacterId;

            _slots[slotA].SetCharacter(b);
            _slots[slotB].SetCharacter(a);
            Touch();
        }

        public IReadOnlyList<int> GetAssignedCharacterIds()
            => _slots.Where(s => s.UserCharacterId.HasValue)
                     .Select(s => s.UserCharacterId!.Value)
                     .ToList();
        private void EnsureSlotCapacity(int slotId)
        {
            if (slotId < 0 )
                throw new ArgumentOutOfRangeException(nameof(slotId), $"Valid range: 0..{_slots.Count - 1}");
            while (_slots.Count <= slotId)
            {
                _slots.Add(new UserPartySlot(PartyId, _slots.Count, null));
            } 
            Touch();
        }
        private void EnsureValidSlot(int slotId)
        {
            if (slotId < 0 || slotId >= _slots.Count)
                throw new ArgumentOutOfRangeException(nameof(slotId), $"Valid range: 0..{_slots.Count - 1}");
        }
        private void Touch() => UpdatedAt = DateTime.UtcNow;
        private UserPartySlot GetSlotById(int slotId)
        {
            // 필요하면 여기서 EnsureSlotCapacity(slotId) 호출
            var slot = _slots.FirstOrDefault(s => s.SlotId == slotId);
            if (slot == null)
            {
                // 없으면 만들어서 추가
                slot = new UserPartySlot(PartyId, slotId, null);
                _slots.Add(slot);
            }
            return slot;
        }

    }
}