using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Session
    {
        private Session() { }

        public int Id { get; private set; }
        public int UserId { get; private set; }

        // 토큰 원문 저장 금지 → 해시만
        public string AccessTokenHash { get; private set; } = default!;
        public string RefreshTokenHash { get; private set; } = default!;

        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset RefreshExpiresAt { get; private set; }

        public bool Revoked { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        public static Session Create(
            int userId,
            string accessTokenHash,
            string refreshTokenHash,
            DateTimeOffset expiresAt,
            DateTimeOffset refreshExpiresAt,
            DateTimeOffset? now = null)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (string.IsNullOrWhiteSpace(accessTokenHash)) throw new ArgumentException("accessTokenHash required");
            if (string.IsNullOrWhiteSpace(refreshTokenHash)) throw new ArgumentException("refreshTokenHash required");
            if (refreshExpiresAt <= expiresAt) throw new ArgumentOutOfRangeException(nameof(refreshExpiresAt));

            return new Session
            {
                UserId = userId,
                AccessTokenHash = accessTokenHash,
                RefreshTokenHash = refreshTokenHash,
                ExpiresAt = expiresAt,
                RefreshExpiresAt = refreshExpiresAt,
                Revoked = false,
                CreatedAt = now ?? DateTimeOffset.UtcNow
            };
        }

        public bool IsAccessExpired(DateTimeOffset? now = null) => (now ?? DateTimeOffset.UtcNow) >= ExpiresAt;
        public bool IsRefreshExpired(DateTimeOffset? now = null) => (now ?? DateTimeOffset.UtcNow) >= RefreshExpiresAt;

        public void Revoke() => Revoked = true;

        public void RotateAccess(string newHash, DateTimeOffset newExpiresAt)
        {
            if (string.IsNullOrWhiteSpace(newHash)) throw new ArgumentException("newHash required");
            AccessTokenHash = newHash;
            ExpiresAt = newExpiresAt;
        }

        public void RotateRefresh(string newHash, DateTimeOffset newRefreshExpiresAt)
        {
            if (string.IsNullOrWhiteSpace(newHash)) throw new ArgumentException("newHash required");
            RefreshTokenHash = newHash;
            RefreshExpiresAt = newRefreshExpiresAt;
        }
    }
}
