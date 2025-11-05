using Application.Repositories;
using Domain.Entities.Contents;
using Domain.Entities.User;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Users
{
    public class UserStageProgressService : IUserStageProgressService
    {
        private readonly IUserStageProgressRepository _repo;
        private readonly ILogger<UserStageProgressService> _logger;

        public UserStageProgressService(
            IUserStageProgressRepository repo,
            ILogger<UserStageProgressService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<IReadOnlyList<UserStageProgress>> GetMyProgressAsync(int userId, CancellationToken ct = default)
        {
            return await _repo.GetByUserIdAsync(userId, ct);
        }

        public async Task<UserStageProgress?> GetProgressAsync(int userId, int stageId, CancellationToken ct = default)
        {
            return await _repo.GetByUserAndStageAsync(userId, stageId, ct);
        }

        public async Task<UserStageProgress> MarkStageFinishedAsync(
            int userId, int stageId, bool success, StageStars stars, DateTime nowUtc, CancellationToken ct = default)
        {
            var progress = await _repo.GetByUserAndStageAsync(userId, stageId, ct);

            if (progress == null)
            {
                progress = new UserStageProgress(userId, stageId);
                await _repo.AddAsync(progress, ct);
            }

            progress.MarkFinish(success, stars, nowUtc);

            await _repo.UpdateAsync(progress, ct);
            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation("User {UserId} cleared stage {StageId} ({Stars})", userId, stageId, stars);
            return progress;
        }
    }
}