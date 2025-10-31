using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Characters
{
    public sealed class CharacterModelWeapon
    {
        public int WeaponId { get; private set; }
        public string Code { get; private set; } = default!;
        public string DisplayName { get; private set; } = default!;
        public bool IsTwoHanded { get; private set; }
         

        private CharacterModelWeapon() { } // EF/Serializer

        public CharacterModelWeapon(int weaponId, string code, string displayName, bool isTwoHanded)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("code required");
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("display_name required");

            WeaponId = weaponId;
            Code = code;
            DisplayName = displayName;
            IsTwoHanded = isTwoHanded; 
        }

        public void Rename(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException(nameof(displayName));
            DisplayName = displayName; 
        }

        public void SetTwoHanded(bool twoHanded)
        {
            IsTwoHanded = twoHanded; 
        } 
    }
} 
