using Contracts.CharacterModel;
using Domain.Entities.Characters;
using Domain.Enum.Characters;
using Google.Protobuf.WellKnownTypes;
using Application.CharacterModels;

namespace WebServer.Mappers
{
    public static class CharacterModelProtoMapper
    {
        public static BodySizePb ToPb(this BodySize v) => v switch
        {
            BodySize.Small => BodySizePb.Small,
            BodySize.Normal => BodySizePb.Normal,
            BodySize.Big => BodySizePb.Big,
            _ => BodySizePb.BodySizeUnspecified
        };
        public static PartTypePb ToPb(this PartType v) => v switch
        {
            PartType.Head => PartTypePb.Head,
            PartType.Hair => PartTypePb.Hair,
            PartType.Mouth => PartTypePb.Mouth,
            PartType.Eye => PartTypePb.Eye,
            PartType.Acc => PartTypePb.Acc,
            PartType.WeaponL => PartTypePb.WeaponL,
            PartType.WeaponR => PartTypePb.WeaponR,
            _ => PartTypePb.PartTypeUnspecified
        };
        public static AnimationTypePb ToPb(this CharacterAnimationType v) => v switch
        {
            CharacterAnimationType.Bow => AnimationTypePb.Bow,
            CharacterAnimationType.OneHandSword => AnimationTypePb.OneHandSword,
            CharacterAnimationType.Wand => AnimationTypePb.Wand,
            CharacterAnimationType.Fist => AnimationTypePb.Fist,
            CharacterAnimationType.TwoHandSword => AnimationTypePb.TwoHandSword,
            CharacterAnimationType.SwordShield => AnimationTypePb.SwordShield,
            CharacterAnimationType.Spear => AnimationTypePb.Spear,
            _ => AnimationTypePb.AnimTypeUnspecified
        };
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
            BodySize = d.BodyType.ToPb(),
            Animation = d.AnimationType.ToPb(),
            WeaponLId = d.WeaponLId.ToW(),
            WeaponRId = d.WeaponRId.ToW(),
            PartHeadId = d.PartHeadId.ToW(),
            PartHairId = d.PartHairId.ToW(),
            PartMouthId = d.PartMouthId.ToW(),
            PartEyeId = d.PartEyeId.ToW(),
            PartAccId = d.PartAccId.ToW(),
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
            };
        }
        public static CharacterModelPartPb ToProto(this Application.CharacterModels.CharacterModelPartDto p) => new()
        {
            PartId = p.PartId,
            PartKey = p.PartKey,
            PartType = p.PartType.ToPb() 
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
