using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(int userId, CancellationToken ct);
        Task AddAsync(UserProfile profile, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
