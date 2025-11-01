using Domain.Enum.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CharacterModels
{
    public sealed class CharacterModelDto
    {
        public int CharacterId { get; set; }
        public string BodyType { get; set; }
        public string AnimationType { get; set; }
        public int? WeaponLId { get; set; }
        public int? WeaponRId { get; set; }
        public int? PartHeadId { get; set; }
        public int? PartHairId { get; set; }
        public int? PartMouthId { get; set; }
        public int? PartEyeId { get; set; }
        public int? PartAccId { get; set; }
    }
    public sealed class CharacterModelPartDto
    {
        public int PartId { get; init; }
        public string PartKey { get; init; } = "";
        public string PartType { get; init; } 
    }
    public sealed class CharacterModelWeaponDto
    {
        public int WeaponId { get; init; }
        public string Code { get; init; } = "";
        public string DisplayName { get; init; } = "";
        public bool IsTwoHanded { get; init; }
    }
    public sealed class CharacterVisualRecipe
    {
        public int CharacterId { get; init; }
        public string BodyType { get; init; } = "";
        public string AnimationType { get; init; } = "";

        public string? WeaponLKey { get; init; }
        public string? WeaponRKey { get; init; }
        public string? HeadKey { get; init; }
        public string? HairKey { get; init; }
        public string? MouthKey { get; init; }
        public string? EyeKey { get; init; }
        public string? AccKey { get; init; }
    }
   
}
