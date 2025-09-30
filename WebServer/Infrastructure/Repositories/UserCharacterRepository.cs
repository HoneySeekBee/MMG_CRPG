using Application.Repositories;
using Domain.Entities.User;
using Infrastructure.Persistence;
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
                  .Include("_skills") // 백킹필드 include
                  .SingleOrDefaultAsync(
                        x => x.UserId == userId && x.CharacterId == characterId, ct);

        public Task AddAsync(UserCharacter entity, CancellationToken ct = default)
            => _db.UserCharacters.AddAsync(entity, ct).AsTask();
    }
}
