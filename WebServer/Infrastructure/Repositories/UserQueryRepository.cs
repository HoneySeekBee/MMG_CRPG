using Application.Repositories;
using Application.Users;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class UserQueryRepository : IUserQueryRepository
    {
        private readonly GameDBContext _db;
        public UserQueryRepository(GameDBContext db) => _db = db;

        public async Task<(IReadOnlyList<(User User, UserProfile Profile)> Rows, int TotalCount)>
     GetPagedAsync(UserListQuery query, CancellationToken ct)
        {
            // base query: Users   UsersProfiles (PK=UserId)
            var q = from u in _db.Users
                    join p in _db.UserProfiles on u.Id equals p.UserId
                    select new { u, p };

            // filters
            if (query.Status is { } st) q = q.Where(x => x.u.Status == st);
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim();

                var pat = $"%{s.ToLower()}%";

                q = q.Where(x =>
                    EF.Functions.Like((x.u.Account ?? string.Empty).ToLower(), pat) ||
                    EF.Functions.Like((x.p.NickName ?? string.Empty).ToLower(), pat));
            }
            if (query.CreatedFrom is { } fromUtc) q = q.Where(x => x.u.CreatedAt >= fromUtc);
            if (query.CreatedTo is { } toUtc) q = q.Where(x => x.u.CreatedAt <= toUtc);

            var total = await q.CountAsync(ct);

            var rows = await q.OrderBy(x => x.u.Id)
                              .Skip((query.Page - 1) * query.PageSize)
                              .Take(query.PageSize)
                              .AsNoTracking()
                              .Select(x => new ValueTuple<User, UserProfile>(x.u, x.p))
                              .ToListAsync(ct);

            return (Rows: rows, TotalCount: total);
        }

        public async Task<(User? User, UserProfile? Profile)>
            GetAggregateAsync(int userId, CancellationToken ct)
        {
            var row = await (from u in _db.Users
                             join p in _db.UserProfiles on u.Id equals p.UserId
                             where u.Id == userId
                             select new { u, p })
                            .AsNoTracking()
                            .SingleOrDefaultAsync(ct);

            return row is null ? (null, null) : (row.u, row.p);
        }
    }
}
