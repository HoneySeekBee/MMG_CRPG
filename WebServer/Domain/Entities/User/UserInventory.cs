using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{
    public sealed class UserInventory
    {
        private UserInventory() { }
        public long Id { get; set; }
        public int UserId { get; set; }
        public int ItemId { get; set; }
        public int Count { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        public static UserInventory Create(int userId, int itemId, int count = 0, DateTimeOffset? now = null)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (itemId <= 0) throw new ArgumentOutOfRangeException(nameof(itemId));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            return new UserInventory
            {
                UserId = userId,
                ItemId = itemId,
                Count = count,
                UpdatedAt = now ?? DateTimeOffset.UtcNow
            };
        }
        public void Add(int amount, DateTimeOffset? when = null)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            checked { Count += amount; }                  // 오버플로 시 예외
            Touch(when);
        }
        public bool TryConsume(int amount, DateTimeOffset? when = null)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (Count < amount) return false;

            Count -= amount;
            Touch(when);
            return true;
        }
        public void SetCount(int count, DateTimeOffset? when = null)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            Count = count;
            Touch(when);
        }

        public void Touch(DateTimeOffset? when = null)
            => UpdatedAt = when ?? DateTimeOffset.UtcNow;
    }
}
