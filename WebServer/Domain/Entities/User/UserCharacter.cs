using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{
    public sealed class UserCharacter
    {
        public int UserId { get; private set; }
        public int CharacterId { get; private set; }
        public int Level { get; private set; }
        public int Exp { get; private set; }
        public int BreakThrough { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        private readonly List<UserCharacterSkill> _skills = new();
        private readonly List<UserCharacterEquip> _equip = new(); 
        public IReadOnlyCollection<UserCharacterSkill> Skills => _skills.AsReadOnly();
        public IReadOnlyCollection<UserCharacterEquip> Equips => _equip.AsReadOnly();
        private UserCharacter() { }
        public static UserCharacter Create(int userId, int cid, DateTimeOffset now)
           => new UserCharacter
           {
               UserId = userId,
               CharacterId = cid,        // short → int 캐스팅 자동
               Level = 1,                // 기본 레벨 1
               Exp = 0,                  // 기본 경험치 0
               BreakThrough = 0,         // 기본 돌파 단계 0
               UpdatedAt = now           // 생성 시점 기록
           }; 
        
        public void LearnSkill(int skillId, DateTimeOffset now)
        {
            if (_skills.Any(s => s.SkillId == skillId)) return;
            _skills.Add(UserCharacterSkill.Create(UserId, CharacterId, skillId, now));
            Touch(now);
        }
        public void LevelUpSkill(int skillId, int amount, DateTimeOffset now)
        {
            var s = _skills.SingleOrDefault(x => x.SkillId == skillId)
                    ?? throw new InvalidOperationException("Skill not learned.");
            // 여기서 캐릭터 레벨 등 규칙 체크
            s.LevelUp(amount, now);
            Touch(now);
        }
        public void GainExp(int amount, DateTimeOffset now)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Exp += amount;
            Touch(now);
        }
        public bool TryLevelUp(int requiredExp, int maxLevel, DateTimeOffset now)
        {
            if (Level >= maxLevel) return false;
            if (Exp < requiredExp) return false;

            Exp -= requiredExp;
            Level += 1;
            Touch(now);
            return true;
        }
        private void Touch(DateTimeOffset now) => UpdatedAt = now;

    }
}
