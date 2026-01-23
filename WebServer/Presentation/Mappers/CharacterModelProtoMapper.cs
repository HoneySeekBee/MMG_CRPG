using Contracts.CharacterModel;
using Domain.Entities.Characters;
using Domain.Enum.Characters;
using Google.Protobuf.WellKnownTypes;
using Application.CharacterModels; 

namespace WebServer.Mappers
{
    public static class CharacterModelProtoMapper
    { 
        public static BodySizePb ToBodyPb(this string s) => s switch
        {
            "Small" => BodySizePb.Small,
            "Normal" => BodySizePb.Normal,
            "Big" => BodySizePb.Big,
            _ => BodySizePb.BodySizeUnspecified
        }; 
        
        public static AnimationTypePb ToAnimPb(this string s) => s switch
        {
            "Bow" => AnimationTypePb.Bow,
            "OneHandSword" => AnimationTypePb.OneHandSword,
            "Wand" => AnimationTypePb.Wand,
            "Fist" => AnimationTypePb.Fist,
            "TwoHandSword" => AnimationTypePb.TwoHandSword,
            "SwordShield" => AnimationTypePb.SwordShield,
            "Spear" => AnimationTypePb.Spear,
            _ => AnimationTypePb.AnimTypeUnspecified
        };

        public static int? ToW(this int? v) => v.HasValue ? v.Value : null;
        public static StringValue? ToW(this string? s) => string.IsNullOrEmpty(s) ? null : new StringValue { Value = s! };
        public static CharacterModelPb ToProto(this Application.CharacterModels.CharacterModelDto d) => new()
        {
            CharacterId = d.CharacterId,
            BodySize = d.BodyType.ToBodyPb(),
            Animation = d.AnimationType.ToAnimPb(),
            WeaponLId = d.WeaponLId.ToW(),
            WeaponRId = d.WeaponRId.ToW(),
            PartHeadId = d.PartHeadId.ToW(),
            PartHairId = d.PartHairId.ToW(),
            PartMouthId = d.PartMouthId.ToW(),
            PartEyeId = d.PartEyeId.ToW(),
            PartAccId = d.PartAccId.ToW(),
            HairColorCode = d.HairColorCode ?? string.Empty,
            SkinColorCode = d.SkinColorCode ?? string.Empty
        };
        public static CharacterModelPb ToProto(this Domain.Entities.Characters.CharacterModel e)
        {
            return new CharacterModelPb
            {
                CharacterId = e.CharacterId,
                BodySize = e.BodyType.ToString().ToBodyPb() ,          // 실제 속성/타입에 맞게 매핑
                Animation = e.AnimationType.ToString().ToAnimPb(),// 실제 속성/타입에 맞게 매핑
                WeaponLId = e.WeaponLId,
                WeaponRId = e.WeaponRId,
                PartHeadId = e.PartHeadId,
                PartHairId = e.PartHairId,
                PartMouthId = e.PartMouthId,
                PartEyeId = e.PartEyeId,
                PartAccId = e.PartAccId,
                HairColorCode = e.HairColorCode ?? string.Empty,
                SkinColorCode = e.SkinColorCode ?? string.Empty
            };
        }
        public static CharacterModelPartPb ToProto(this Application.CharacterModels.CharacterModelPartDto p) => new()
        {
            PartId = p.PartId,
            PartKey = p.PartKey,
            PartType = (PartTypePb)System.Enum.Parse(typeof(PartTypePb), p.PartType)
        };

        public static CharacterModelWeaponPb ToProto(this Application.CharacterModels.CharacterModelWeaponDto w) => new()
        {
            WeaponId = w.WeaponId,
            Code = w.Code,
            DisplayName = w.DisplayName,
            IsTwoHanded = w.IsTwoHanded
        };

        public static CharacterVisualRecipePb ToProto(this Application.CharacterModels.CharacterVisualRecipe r) => new()
        {
            CharacterId = r.CharacterId,
            BodySize = r.BodyType.ToBodyPb(),
            Animation = r.AnimationType.ToAnimPb(),
            WeaponLKey = r.WeaponLKey,
            WeaponRKey = r.WeaponRKey,
            HeadKey = r.HeadKey,
            HairKey = r.HairKey,
            MouthKey = r.MouthKey,
            EyeKey = r.EyeKey,
            AccKey = r.AccKey
        }; 
    }
}
