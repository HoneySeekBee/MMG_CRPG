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

        public async Task<UserStageProgress> MarkStageFinishedAsync(int userId, int stageId, bool success, StageStars stars, DateTime nowUtc, CancellationToken ct = default)
        {
            var progress = await _repo.GetByUserAndStageAsync(userId, stageId, ct);

            bool isNew = false;

            if (progress == null)
            {
                // 신규 진행도 생성
                progress = new UserStageProgress(userId, stageId);
                isNew = true;

                // 먼저 도메인 상태 갱신
                progress.MarkFinish(success, stars, nowUtc);

                await _repo.AddAsync(progress, ct);
            }
            else
            {
                // 기존 진행도에 도메인 상태만 갱신
                progress.MarkFinish(success, stars, nowUtc);

                // 이미 트래킹 중이면 Update조차 필요 없을 수도 있지만,
                // 리포지토리 구현에 따라 다르니 남겨두는 쪽으로
                await _repo.UpdateAsync(progress, ct);
            }

            await _repo.SaveChangesAsync(ct);

            _logger.LogInformation(
                "User {UserId} cleared stage {StageId} ({Stars})",
                userId, stageId, stars);

            return progress;
        }
    }
}