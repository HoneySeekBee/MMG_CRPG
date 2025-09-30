using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{
    public sealed class UserProfile
    {
        private UserProfile() { }

        public int Id { get; private set; }
        public int UserId { get; private set; }

        public string NickName { get; private set; } = default!;
        public short Level { get; private set; } = 1;
        public int Exp { get; private set; } = 0;

        // 초기엔 필드로 두되, 추후 통화 테이블로 분리 가능
        public int Gold { get; private set; } = 0;
        public int Gem { get; private set; } = 0;
        public int Token { get; private set; } = 0;

        public int? IconId { get; private set; }

        public static UserProfile Create(int userId, string nickName, int? iconId = null)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (string.IsNullOrWhiteSpace(nickName))
                throw new ArgumentException("nickName is required", nameof(nickName));
            if (nickName.Length > 100)
                throw new ArgumentOutOfRangeException(nameof(nickName), "max length 100");

            return new UserProfile
            {
                UserId = userId,
                NickName = nickName.Trim(),
                IconId = iconId
            };
        }

        internal void BindToUser(int userId) => UserId = userId;

        public void Rename(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick) || nick.Length > 100)
                throw new ArgumentOutOfRangeException(nameof(nick));
            NickName = nick.Trim();
        }

        public void AddExp(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Exp += amount;
            // 레벨업 규칙은 Application에서 성장 곡선/테이블을 사용해 처리
        }

        public void SetIcon(int? iconId) => IconId = iconId;
        public void ResetProgress() { Level = 1; Exp = 0; }
    }
}
