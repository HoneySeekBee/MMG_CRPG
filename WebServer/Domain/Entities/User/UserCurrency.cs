using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.User
{
    public sealed class UserCurrency
    {
        public int UserId { get; private set; }
        public int CurrencyId { get; private set; }
        public long Amount { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        private UserCurrency() { }
        public static UserCurrency Create(int userId, int cid, DateTimeOffset now)
            => new() { UserId = userId, CurrencyId = cid, Amount = 0, UpdatedAt = now };
        public void Grant(long delta, DateTimeOffset now) { checked { Amount += delta; } if (Amount < 0) Amount = 0; UpdatedAt = now; }
        public bool Spend(long delta, DateTimeOffset now) { if (delta <= 0 || Amount < delta) return false; Amount -= delta; UpdatedAt = now; return true; }
    }
}
