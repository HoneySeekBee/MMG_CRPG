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

        public async Task AddRangeAsync(IEnumerable<UserCharacter> entities, CancellationToken ct = default)
            => await _db.UserCharacters.AddRangeAsync(entities, ct);

        public async Task<HashSet<int>> GetOwnedCharacterIdsAsync(int userId, IEnumerable<int> characterIds, CancellationToken ct = default)
        {
            var ids = characterIds.ToList();
            if (ids.Count == 0) return new HashSet<int>();

            var owned = await _db.UserCharacters
                .AsNoTracking()
                .Where(x => x.UserId == userId && ids.Contains(x.CharacterId))
                .Select(x => x.CharacterId)
                .ToListAsync(ct);

            return owned.ToHashSet();
        }

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
