using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class SecurityEvent
    {
        private SecurityEvent() { }

        public int Id { get; private set; }
        public int? UserId { get; private set; }
        public SecurityEventType Type { get; private set; }
        public string? Meta { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        public static SecurityEvent Create(SecurityEventType type, DateTimeOffset? when = null, int? userId = null, string? metaJson = null)
        {
            return new SecurityEvent
            {
                Type = type,
                CreatedAt = when ?? DateTimeOffset.UtcNow,
                UserId = userId,
                Meta = metaJson
            };
        }
    }
}
