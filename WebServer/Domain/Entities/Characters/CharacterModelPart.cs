using Domain.Enum.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Characters
{
    public sealed class CharacterModelPart
    {
        public int PartId { get; private set; }
        public string PartKey { get; private set; } = default!;   // Addressables 키
        public string PartType { get; private set; } 
        private CharacterModelPart() { }

        public CharacterModelPart(int partId, string partKey, string partType )
        {
            if (string.IsNullOrWhiteSpace(partKey)) throw new ArgumentException(nameof(partKey));
            PartId = partId;
            PartKey = partKey;
            PartType = partType; 
        }

        public void ChangeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));
            PartKey = key; 
        }
         

    }
}
