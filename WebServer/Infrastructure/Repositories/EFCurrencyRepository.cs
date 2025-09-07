using Application.Repositories;
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
    }
}
