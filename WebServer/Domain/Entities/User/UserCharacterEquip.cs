using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{
    public sealed class UserCharacterEquip
    {
        public int UserId{ get; private set; }
        public int CharacterId{ get; private set; }
        public short SlotId { get; private set; }
        public int? ItemId { get; private set; }
        private UserCharacterEquip(){ }

        public static UserCharacterEquip Create(int uesrId, int characterId, short slotId, int? itemId)
            => new() { UserId = uesrId, CharacterId = characterId, SlotId = slotId, ItemId = itemId };


        public void Equip(int itemId) => ItemId = itemId;
        public void Unequip() => ItemId = null;
    }
}
