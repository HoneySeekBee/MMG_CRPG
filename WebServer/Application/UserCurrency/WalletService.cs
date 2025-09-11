using Application.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserCurrency
{
    public sealed class WalletService : IWalletService
    {
        private readonly ICurrencyRepository _cur;
        private readonly IUserCurrencyRepository _userCur;
        private readonly IClock _clock;
        public WalletService(ICurrencyRepository c, IUserCurrencyRepository u, IClock clock) 
        { 
            _cur = c; 
            _userCur = u; 
            _clock = clock;
        }

        public async Task<List<(string Code, long Amount)>> GetBalancesAsync(int userId, CancellationToken ct)
        {
            var masters = await _cur.GetAllAsync(ct);                 // IReadOnlyList<Currency>
            var rows = await _userCur.GetByUserAsync(userId, ct);     // List<UserCurrency>
            var byId = rows?.ToDictionary(r => r.CurrencyId, r => r.Amount)
                       ?? new Dictionary<short, long>();

            var result = new List<(string Code, long Amount)>(masters.Count);
            foreach (var m in masters)
            {
                byId.TryGetValue(m.Id, out var amt);              
                result.Add((m.Code, amt));
            }
            return result;
        }

        public async Task GrantAsync(int userId, string code, long amount, CancellationToken ct)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

            var m = await _cur.FindByCodeAsync(code, ct)
                    ?? throw new InvalidOperationException("CURRENCY_NOT_FOUND");

            var row = await _userCur.GetAsync(userId, m.Id, ct);
            if (row is null)
            {
                // 새 지갑행 생성 후 지급
                row = Domain.Entities.UserCurrency.Create(userId, m.Id, _clock.UtcNow);
                row.Grant(amount, _clock.UtcNow);
                await _userCur.AddAsync(row, ct);
            }
            else
            {
                row.Grant(amount, _clock.UtcNow);
            }

            await _userCur.SaveChangesAsync(ct);
        }

        public async Task<bool> SpendAsync(int userId, string code, long amount, CancellationToken ct)
        {
            if (amount <= 0) return false;

            var m = await _cur.FindByCodeAsync(code, ct)
                    ?? throw new InvalidOperationException("CURRENCY_NOT_FOUND");

            var row = await _userCur.GetAsync(userId, m.Id, ct);
            if (row is null) return false;

            var ok = row.Spend(amount, _clock.UtcNow);
            if (!ok) return false;

            await _userCur.SaveChangesAsync(ct);
            return true;
        }
    }
}
