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
        public PartType PartType { get; private set; }
        public BodySize? BodyType { get; private set; }           // null = 모든 체형 호환
          
        private CharacterModelPart() { }

        public CharacterModelPart(int partId, string partKey, PartType partType, BodySize? bodyType = null)
        {
            if (string.IsNullOrWhiteSpace(partKey)) throw new ArgumentException(nameof(partKey));
            PartId = partId;
            PartKey = partKey;
            PartType = partType;
            BodyType = bodyType; 
        }

        public void ChangeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException(nameof(key));
            PartKey = key; 
        }

        public void SetCompatibleBody(BodySize? size)
        {
            BodyType = size; 
        }
         

    }
}
