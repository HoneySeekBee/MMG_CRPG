using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserCurrency
{
    public interface IWalletService
    {
        Task<List<(string Code, long Amount)>> GetBalancesAsync(int userId, CancellationToken ct);
        Task GrantAsync(int userId, string code, long amount, CancellationToken ct);
        Task<bool> SpendAsync(int userId, string code, long amount, CancellationToken ct);
    }
}
