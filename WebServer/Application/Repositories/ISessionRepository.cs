using Application.Users;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface ISessionQueryRepository
    {
        /// <summary>세션 페이징(운영툴)</summary>
        Task<(IReadOnlyList<Session> Items, int TotalCount)>
            GetPagedAsync(SessionListQuery query, CancellationToken ct);

        /// <summary>최근 세션 N개(상세 화면)</summary>
        Task<IReadOnlyList<Session>> GetRecentByUserIdAsync(int userId, int take, CancellationToken ct);
    }
    public interface ISessionRepository
    {
        Task AddAsync(Session session, CancellationToken ct);
        Task<Session?> FindByRefreshHashAsync(string refreshHash, CancellationToken ct);
        Task<Session?> FindByIdAsync(int sessionId, CancellationToken ct);
        Task InvalidateAsync(Session session, CancellationToken ct);
        Task InvalidateAllByUserIdAsync(int userId, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
