using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CharacterModels
{
    public sealed class CreateCharacterModelRequest
    {
        public string BodyType { get; init; } = "";
        public string AnimationType { get; init; } = "";
        public int? WeaponLId { get; init; }
        public int? WeaponRId { get; init; }
        public int? PartHeadId { get; init; }
        public int? PartHairId { get; init; }
        public int? PartMouthId { get; init; }
        public int? PartEyeId { get; init; }
        public int? PartAccId { get; init; }
    }
}
