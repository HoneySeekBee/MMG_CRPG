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
    public class UserCharacterEquipRepository : IUserCharacterEquipRepository
    {
        private readonly GameDBContext _db;
        public UserCharacterEquipRepository(GameDBContext db) => _db = db;

        public Task<UserCharacterEquip?> GetAsync(int userId, int characterId, int equipId, CancellationToken ct = default)
       => _db.UserCharacterEquips.FirstOrDefaultAsync(
           x => x.UserId == userId && x.CharacterId == characterId && x.EquipId == equipId,
           ct);

        public Task<List<UserCharacterEquip>> ListByCharacterAsync(int userId, int characterId, CancellationToken ct = default)
            => _db.UserCharacterEquips
                  .Where(x => x.UserId == userId && x.CharacterId == characterId)
                  .ToListAsync(ct);

        public Task<UserCharacterEquip?> FindByInventoryIdAsync(long inventoryId, CancellationToken ct = default)
            => _db.UserCharacterEquips.FirstOrDefaultAsync(
                x => x.InventoryId == inventoryId,
                ct);

        public Task AddAsync(UserCharacterEquip entity, CancellationToken ct = default)
            => _db.UserCharacterEquips.AddAsync(entity, ct).AsTask();

        public Task UpdateAsync(UserCharacterEquip entity, CancellationToken ct = default)
        {
            _db.UserCharacterEquips.Update(entity);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
