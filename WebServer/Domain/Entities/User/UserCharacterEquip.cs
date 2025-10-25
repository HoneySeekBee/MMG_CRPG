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
        public int EquipId { get; private set; }
        public int? ItemId { get; private set; }
        public UserCharacter UserCharacter { get; private set; } = default!;
        private UserCharacterEquip(){ }

        public static UserCharacterEquip Create(int uesrId, int characterId, int equipId, int? itemId)
            => new() { UserId = uesrId, CharacterId = characterId, EquipId = equipId, ItemId = itemId };


        public void Equip(int itemId) => ItemId = itemId;
        public void Unequip() => ItemId = null;
    }
}
