using Domain.Enum;
using SkillSlot = Domain.Enum.SkillSlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class CharacterSkill
    {
        // EF Core용
        private CharacterSkill() { }

        // ==== Key ====
        public int CharacterId { get; private set; }
        public SkillSlot Slot { get; private set; }    // 키 일부 → 불변

        // ==== Ref ====
        public int SkillId { get; private set; }       // Skills 테이블 FK

        // ==== Unlock Rules ====
        public short UnlockTier { get; private set; } = 0; // >= 0
        public short UnlockLevel { get; private set; } = 1; // >= 1

        // ==== Navigation ====
        public Character Character { get; private set; } = null!;
        // public Skill Skill { get; private set; } = null!; // 도메인에 Skill 엔티티가 있으면 사용

        // ==== Factory ====
        public static CharacterSkill Create(
            int characterId,
            SkillSlot slot,
            int skillId,
            short unlockTier = 0,
            short unlockLevel = 1)
        {
            if (!System.Enum.IsDefined(typeof(SkillSlot), slot))
                throw new ArgumentOutOfRangeException(nameof(slot));
            if (skillId <= 0)
                throw new ArgumentOutOfRangeException(nameof(skillId));
            if (unlockTier < 0)
                throw new ArgumentOutOfRangeException(nameof(unlockTier));
            if (unlockLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(unlockLevel));

            return new CharacterSkill
            {
                CharacterId = characterId,
                Slot = slot,
                SkillId = skillId,
                UnlockTier = unlockTier,
                UnlockLevel = unlockLevel
            };
        }

        // ==== Mutators ====
        public void SetSkill(int skillId)
        {
            if (skillId <= 0) throw new ArgumentOutOfRangeException(nameof(skillId));
            SkillId = skillId;
        }

        public void SetUnlock(short tier, short level)
        {
            if (tier < 0) throw new ArgumentOutOfRangeException(nameof(tier));
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            UnlockTier = tier;
            UnlockLevel = level;
        }
    }
}
