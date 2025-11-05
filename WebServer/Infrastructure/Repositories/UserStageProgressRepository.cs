using Application.Repositories;
using Domain.Entities.User;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class UserStageProgressRepository : IUserStageProgressRepository
    {
        private readonly GameDBContext _db;

        public UserStageProgressRepository(GameDBContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<UserStageProgress>> GetByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _db.UserStageProgresses
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .ToListAsync(ct);
        }

        public async Task<UserStageProgress?> GetByUserAndStageAsync(int userId, int stageId, CancellationToken ct = default)
        {
            return await _db.UserStageProgresses
                .FirstOrDefaultAsync(x => x.UserId == userId && x.StageId == stageId, ct);
        }

        public async Task AddAsync(UserStageProgress entity, CancellationToken ct = default)
        {
            await _db.UserStageProgresses.AddAsync(entity, ct);
        }

        public Task UpdateAsync(UserStageProgress entity, CancellationToken ct = default)
        {
            _db.UserStageProgresses.Update(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}
