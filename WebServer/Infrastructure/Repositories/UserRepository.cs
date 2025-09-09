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
    public sealed class UserRepository : IUserRepository
    {
        private readonly GameDBContext _db;
        public UserRepository(GameDBContext db) => _db = db;

        public Task<bool> ExistsByAccountAsync(string account, CancellationToken ct)
            => _db.Users.AnyAsync(x => x.Account == account, ct);

        public Task<User?> FindByAccountAsync(string account, CancellationToken ct)
            => _db.Users.FirstOrDefaultAsync(x => x.Account == account, ct);

        public Task<User?> GetByIdAsync(int userId, CancellationToken ct)
            => _db.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);

        public Task AddAsync(User user, CancellationToken ct)
        {
            _db.Users.Add(user);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
