using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{
    public sealed class User
    {
        private User() { }

        public int Id { get; private set; }

        // 로그인 식별자/자격
        public string Account { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!; // 평문 금지

        // 상태/시간
        public UserStatus Status { get; private set; } = UserStatus.Active;
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? LastLoginAt { get; private set; }

        // 선택: 1:1 프로필 참조(ORM에서 관계 매핑)
        public UserProfile? Profile { get; private set; }

        public static User Create(string account, string passwordHash, DateTimeOffset? now = null)
        {
            if (string.IsNullOrWhiteSpace(account))
                throw new ArgumentException("account is required", nameof(account));
            if (account.Length is < 4 or > 64)
                throw new ArgumentOutOfRangeException(nameof(account), "account length 4~64");
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("passwordHash is required", nameof(passwordHash));

            return new User
            {
                Account = account.Trim(),
                PasswordHash = passwordHash,
                Status = UserStatus.Active,
                CreatedAt = now ?? DateTimeOffset.UtcNow
            };
        }

        public void SetPasswordHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("hash is required", nameof(hash));
            PasswordHash = hash;
        }

        public void TouchLogin(DateTimeOffset? when = null) => LastLoginAt = when ?? DateTimeOffset.UtcNow;

        public void Suspend() => Status = UserStatus.Suspended;
        public void Activate() => Status = UserStatus.Active;
        public void MarkDeleted() => Status = UserStatus.Deleted;

        // 프로필 연결(1:1 보장, 필요한 경우만 사용)
        public void AttachProfile(UserProfile profile)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            if (profile.UserId != Id) profile.BindToUser(Id);
        }
    }
}
