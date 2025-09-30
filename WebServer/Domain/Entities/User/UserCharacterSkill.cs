using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{
    public sealed class UserCharacterSkill
    {
        public int UserId { get; private set; }
        public int CharacterId { get; private set; }
        public int SkillId { get; private set; }
        public int Level { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        public UserCharacter UserCharacter { get; private set; } = default!;
        private UserCharacterSkill() { }

        public static UserCharacterSkill Create(int userId, int characterId, int skillId,
                                          DateTimeOffset now, int level = 1)
      => new()
      {
          UserId = userId,
          CharacterId = characterId,
          SkillId = skillId,
          Level = level,          // 기본 1레벨
          UpdatedAt = now
      };

        public void LevelUp(int amount, DateTimeOffset now)
        {
            if (amount <= 0) return;
            Level += amount;
            UpdatedAt = now;
        }

        public void SetLevel(int level, DateTimeOffset now)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            Level = level;
            UpdatedAt = now;
        }

        public void Touch(DateTimeOffset now) => UpdatedAt = now;

    }
}
