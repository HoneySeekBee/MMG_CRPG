using Application.Repositories;
using Application.UserCurrency;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class EFCurrencyRepository : ICurrencyRepository
    {
        private readonly GameDBContext _db;
        public EFCurrencyRepository(GameDBContext db) => _db = db;

        public async Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct) =>
            await _db.Currencies
                     .OrderBy(c => c.Code)
                     .AsNoTracking()
                     .ToListAsync(ct);

        public Task<Currency?> GetByIdAsync(short id, CancellationToken ct) =>
            _db.Currencies.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<Currency?> FindByCodeAsync(string code, CancellationToken ct) =>
            _db.Currencies.FirstOrDefaultAsync(x => x.Code == code, ct);

        public Task AddAsync(Currency row, CancellationToken ct)
        {
            _db.Currencies.Add(row);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
