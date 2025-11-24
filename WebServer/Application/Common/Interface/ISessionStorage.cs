using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interface
{
    public interface ISessionStorage
    {
        Task StoreSessionAsync(Session session, CancellationToken ct);
        Task<Session?> GetByRefreshHashAsync(string refreshHash, CancellationToken ct);
        Task RevokeAsync(string refreshHash, CancellationToken ct);
        Task RevokeAllByUserIdAsync(int userId);
    }
}
