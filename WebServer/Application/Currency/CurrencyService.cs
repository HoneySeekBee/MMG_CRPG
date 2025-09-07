using Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Currency
{
    public sealed class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _repo;
        public CurrencyService(ICurrencyRepository repo) => _repo = repo;

        public async Task<IReadOnlyList<CurrencyDto>> ListAsync(CancellationToken ct)
        {
            var list = await _repo.GetAllAsync(ct);
            return list.Select(c => new CurrencyDto(c.Id, c.Code, c.Name)).ToList();
        }
    }
}
