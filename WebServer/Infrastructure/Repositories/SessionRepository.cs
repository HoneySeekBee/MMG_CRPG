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
    public sealed class SessionRepository : ISessionRepository
    {
        private readonly GameDBContext _db;
        public SessionRepository(GameDBContext db) => _db = db;

        public Task AddAsync(Session session, CancellationToken ct)
        {
            _db.Sessions.Add(session);
            return Task.CompletedTask;
        }

        public Task<Session?> FindByRefreshHashAsync(string refreshHash, CancellationToken ct)
            => _db.Sessions.FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshHash, ct);

        public Task<Session?> FindByIdAsync(int sessionId, CancellationToken ct)
            => _db.Sessions.FirstOrDefaultAsync(x => x.Id == sessionId, ct);

        public Task InvalidateAsync(Session session, CancellationToken ct)
        {
            session.Revoke();
            return Task.CompletedTask;
        }

        public async Task InvalidateAllByUserIdAsync(int userId, CancellationToken ct)
        {
            var sessions = await _db.Sessions.Where(s => s.UserId == userId && !s.Revoked).ToListAsync(ct);
            foreach (var s in sessions) s.Revoke();
        }

        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
