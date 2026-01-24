using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public static class RedisSessionMapper
    {
        public static RedisSessionDto ToDto(this Session s) => new()
        {
            UserId = s.UserId,
            AccessTokenHash = s.AccessTokenHash,
            RefreshTokenHash = s.RefreshTokenHash,
            ExpiresAt = s.ExpiresAt,
            RefreshExpiresAt = s.RefreshExpiresAt,
            Revoked = s.Revoked,
            CreatedAt = s.CreatedAt,
            RevokedAt = s.RevokedAt
        };

        public static Session ToDomain(this RedisSessionDto d)
        { 
            var s = Session.Rehydrate(
                userId: d.UserId,
                accessTokenHash: d.AccessTokenHash,
                refreshTokenHash: d.RefreshTokenHash,
                expiresAt: d.ExpiresAt,
                refreshExpiresAt: d.RefreshExpiresAt,
                revoked: d.Revoked,
                createdAt: d.CreatedAt,
                revokedAt: d.RevokedAt
            );
            return s;
        }
    }
}
