using Application.Repositories;
using Application.Users;
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
    public sealed class SessionQueryRepository : ISessionQueryRepository
    {
        private readonly GameDBContext _db;
        public SessionQueryRepository(GameDBContext db) => _db = db;

        public async Task<(IReadOnlyList<Session> Items, int TotalCount)>
            GetPagedAsync(SessionListQuery query, CancellationToken ct)
        {
            var q = _db.Sessions.AsQueryable();

            if (query.UserId.HasValue)
                q = q.Where(s => s.UserId == query.UserId.Value);

            if (query.Revoked.HasValue)
                q = q.Where(s => s.Revoked == query.Revoked.Value);

            if (query.ActiveOnly)
                q = q.Where(s => !s.Revoked && s.ExpiresAt > DateTimeOffset.UtcNow);

            var total = await q.CountAsync(ct);

            var items = await q.OrderByDescending(s => s.CreatedAt)
                               .Skip((query.Page - 1) * query.PageSize)
                               .Take(query.PageSize)
                               .AsNoTracking()
                               .ToListAsync(ct);

            return (items, total);
        }

        public Task<IReadOnlyList<Session>> GetRecentByUserIdAsync(int userId, int take, CancellationToken ct)
            => _db.Sessions.Where(s => s.UserId == userId)
                           .OrderByDescending(s => s.CreatedAt)
                           .Take(take)
                           .AsNoTracking()
                           .ToListAsync(ct)
                           .ContinueWith<IReadOnlyList<Session>>(t => t.Result, ct);
    }
}
