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
    public sealed class UserCharacterRepository : IUserCharacterRepository
    {
        private readonly GameDBContext _db;
        public UserCharacterRepository(GameDBContext db) => _db = db;

        public Task<UserCharacter?> GetAsync(int userId, int characterId, CancellationToken ct = default)
            => _db.UserCharacters
            .Include(x => x.Skills)
            .Include(x => x.Equips)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CharacterId == characterId, ct);

        public Task AddAsync(UserCharacter entity, CancellationToken ct = default)
            => _db.UserCharacters.AddAsync(entity, ct).AsTask();

        public async Task<(IReadOnlyList<UserCharacter> Items, int TotalCount)> GetListAsync(int userId, int page, int pageSize, CancellationToken ct = default)
        {
            var q = _db.UserCharacters
                       .AsNoTracking()
                       .Include(x => x.Skills)
                       .Include(x => x.Equips)
                       .Where(x => x.UserId == userId)
                       .OrderBy(x => x.CharacterId)
                       .AsSplitQuery();

            var total = await q.CountAsync(ct);
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return (items, total);
        }
    }
}
