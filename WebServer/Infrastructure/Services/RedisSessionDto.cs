using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public sealed class RedisSessionDto
    {
        public int UserId { get; set; }
        public string AccessTokenHash { get; set; } = "";
        public string RefreshTokenHash { get; set; } = "";
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset RefreshExpiresAt { get; set; }
        public bool Revoked { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
    }

}
