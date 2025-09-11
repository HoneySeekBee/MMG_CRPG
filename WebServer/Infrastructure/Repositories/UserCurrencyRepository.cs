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
    public sealed class UserCurrencyRepository : IUserCurrencyRepository
    {
        private readonly GameDBContext _db; 
        public UserCurrencyRepository(GameDBContext db) => _db = db;
        public Task<UserCurrency?> GetAsync(int userId, short cid, CancellationToken ct) =>
            _db.UserCurrencies.FirstOrDefaultAsync(x => x.UserId == userId && x.CurrencyId == cid, ct);
        public Task<List<UserCurrency>> GetByUserAsync(int userId, CancellationToken ct) =>
            _db.UserCurrencies.Where(x => x.UserId == userId).ToListAsync(ct);
        public Task AddAsync(UserCurrency row, CancellationToken ct) { _db.UserCurrencies.Add(row); return Task.CompletedTask; }
        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
        public async Task InitializeForUserAsync(int userId, CancellationToken ct)
        {
            var sql = """
        INSERT INTO "UserCurrency" ("UserId","CurrencyId","Amount","UpdatedAt")
        SELECT @p0, c."Id", 0, NOW()
        FROM "Currencies" c
        ON CONFLICT ("UserId","CurrencyId") DO NOTHING
        """;

            await _db.Database.ExecuteSqlRawAsync(sql, new object[] { userId }, ct);
        }

        public async Task GrantAsync(int userId, string code, long amount, CancellationToken ct)
        {
            var sql = """
        INSERT INTO "UserCurrency" ("UserId","CurrencyId","Amount","UpdatedAt")
        SELECT @p0, c."Id", @p1, NOW()
        FROM "Currencies" c
        WHERE c."Code" = @p2
        ON CONFLICT ("UserId","CurrencyId")
        DO UPDATE SET "Amount" = "UserCurrency"."Amount" + EXCLUDED."Amount",
                      "UpdatedAt" = NOW()
        """;
            await _db.Database.ExecuteSqlRawAsync(sql, new object[] { userId, amount, code }, ct);
        }
    }
}
