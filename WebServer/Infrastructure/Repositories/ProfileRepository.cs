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
    public sealed class ProfileRepository : IProfileRepository
    {
        private readonly GameDBContext _db;
        public ProfileRepository(GameDBContext db) => _db = db;

        public Task<UserProfile?> GetByUserIdAsync(int userId, CancellationToken ct)
            => _db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);

        public Task AddAsync(UserProfile profile, CancellationToken ct)
        {
            _db.UserProfiles.Add(profile);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}

