using Domain.Enum.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Characters
{
    public sealed class CharacterModel
    {
        public int CharacterId { get; private set; }

        public string BodyType { get; private set; }
        public string AnimationType { get; private set; }

        // 무기 슬롯 (양손 검 등은 L만 세팅하고 R은 null)
        public int? WeaponLId { get; private set; }
        public int? WeaponRId { get; private set; }

        // 파츠 슬롯
        public int? PartHeadId { get; private set; }
        public int? PartHairId { get; private set; }
        public int? PartMouthId { get; private set; }
        public int? PartEyeId { get; private set; }
        public int? PartAccId { get; private set; }
        public string HairColorCode{ get; private set; }
        public string SkinColorCode{ get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private CharacterModel() { } // EF/Serializer

        private CharacterModel(int characterId, BodySize body, CharacterAnimationType anim)
        {
            CharacterId = characterId;
            BodyType = body.ToString();
            AnimationType = anim.ToString();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public static CharacterModel Create(int characterId, BodySize body, CharacterAnimationType anim)
            => new CharacterModel(characterId, body, anim);

        public void SetBody(BodySize body)
        {
            BodyType = body.ToString();
            Touch();
        }

        public void SetAnimation(CharacterAnimationType type)
        {
            AnimationType = type.ToString();
            Touch();
        }

        /// <summary>무기 장착(좌/우). 두손무기면 right를 자동 해제하도록 Application에서 검증 후 호출.</summary>
        public void EquipWeapons(int? leftWeaponId, int? rightWeaponId)
        {
            WeaponLId = leftWeaponId;
            WeaponRId = rightWeaponId;
            Touch();
        }

        public void EquipTwoHanded(int weaponId)
        {
            WeaponLId = weaponId;
            WeaponRId = null;
            Touch();
        }

        public void ClearWeapons()
        {
            WeaponLId = null;
            WeaponRId = null;
            Touch();
        }
        public void Update(BodySize body, CharacterAnimationType anim, int? weaponLId, int? weaponRId, int? partHeadId, int? partHairId, int? partMouthId, int? partEyeId, int? partAccId)
        {
            BodyType = body.ToString();
            AnimationType = anim.ToString();

            // 무기 장착 갱신
            WeaponLId = weaponLId;
            WeaponRId = weaponRId;

            // 파츠 갱신
            PartHeadId = partHeadId;
            PartHairId = partHairId;
            PartMouthId = partMouthId;
            PartEyeId = partEyeId;
            PartAccId = partAccId;

            Touch(); // UpdatedAt = DateTime.UtcNow;
        }

        public void SetPart(PartType type, int? partId)
        {
            switch (type)
            {
                case PartType.Head: PartHeadId = partId; break;
                case PartType.Hair: PartHairId = partId; break;
                case PartType.Mouth: PartMouthId = partId; break;
                case PartType.Eye: PartEyeId = partId; break;
                case PartType.Acc: PartAccId = partId; break;
                case PartType.WeaponL: WeaponLId = partId; break; // 무기를 “파츠처럼” 다룰 때를 위한 백도어
                case PartType.WeaponR: WeaponRId = partId; break;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
            Touch();
        }

        public int? GetPartId(PartType type) => type switch
        {
            PartType.Head => PartHeadId,
            PartType.Hair => PartHairId,
            PartType.Mouth => PartMouthId,
            PartType.Eye => PartEyeId,
            PartType.Acc => PartAccId,
            PartType.WeaponL => WeaponLId,
            PartType.WeaponR => WeaponRId,
            _ => null
        };

        private void Touch() => UpdatedAt = DateTime.UtcNow;
    }
}