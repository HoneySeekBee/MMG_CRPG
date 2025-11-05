using Domain.Entities.Contents;
using Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Users
{
    public interface IUserStageProgressService
    { 
        Task<IReadOnlyList<UserStageProgress>> GetMyProgressAsync(int userId, CancellationToken ct = default);
         
        Task<UserStageProgress?> GetProgressAsync(int userId, int stageId, CancellationToken ct = default);
         
        Task<UserStageProgress> MarkStageFinishedAsync(int userId, int stageId, bool success, StageStars stars, DateTime nowUtc, CancellationToken ct = default);
    }
}
