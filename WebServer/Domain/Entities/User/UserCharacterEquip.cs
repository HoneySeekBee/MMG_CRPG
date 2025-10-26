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
        public long? InventoryId { get; private set; }
        public UserCharacter UserCharacter { get; private set; } = default!;
        private UserCharacterEquip(){ }

        public static UserCharacterEquip Create(int uesrId, int characterId, int equipId, long? inventoryId)
            => new() { UserId = uesrId, CharacterId = characterId, EquipId = equipId, InventoryId = inventoryId };


        public void Equip(long inventoryId) => InventoryId = inventoryId;
        public void Unequip() => InventoryId = null;
    }
}
