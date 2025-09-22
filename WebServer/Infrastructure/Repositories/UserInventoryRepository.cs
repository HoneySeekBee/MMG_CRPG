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
    public sealed class UserInventoryRepository : IUserInventoryRepository
    {
        private readonly GameDBContext _db;
        public UserInventoryRepository(GameDBContext db) => _db = db;

        public Task<UserInventory?> GetByKeyAsync(int userId, int itemId, CancellationToken ct)
            => _db.UserInventories
                  .FirstOrDefaultAsync(x => x.UserId == userId && x.ItemId == itemId, ct);

        public Task AddAsync(UserInventory entity, CancellationToken ct)
        {
            _db.UserInventories.Add(entity);
            return Task.CompletedTask;
        }

        public void Remove(UserInventory entity)
        {
            _db.UserInventories.Remove(entity);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
