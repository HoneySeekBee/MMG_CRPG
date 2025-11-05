using Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IUserStageProgressRepository
    {
        Task<IReadOnlyList<UserStageProgress>> GetByUserIdAsync(int userId, CancellationToken ct = default);
        Task<UserStageProgress?> GetByUserAndStageAsync(int userId, int stageId, CancellationToken ct = default);
        Task AddAsync(UserStageProgress entity, CancellationToken ct = default);
        Task UpdateAsync(UserStageProgress entity, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);

    }
}
