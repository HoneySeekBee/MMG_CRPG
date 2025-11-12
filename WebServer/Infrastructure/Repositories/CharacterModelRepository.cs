using Application.Repositories;
using Domain.Entities.Characters;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class CharacterModelRepository : ICharacterModelRepository
    {
        private readonly GameDBContext _db;

        public CharacterModelRepository(GameDBContext db) => _db = db;

        public async Task<CharacterModel?> GetByCharacterIdAsync(int characterId, CancellationToken ct)
            => await _db.Set<CharacterModel>().FirstOrDefaultAsync(x => x.CharacterId == characterId, ct);

        public async Task<List<CharacterModel>> GetAllAsync(CancellationToken ct)
            => await _db.Set<CharacterModel>().ToListAsync(ct);

        public async Task<CharacterModel> AddAsync(CharacterModel model, CancellationToken ct)
        {
            _db.Add(model);
            await _db.SaveChangesAsync(ct);
            return model;
        }

        public async Task UpdateAsync(CharacterModel model, CancellationToken ct)
        {
            _db.Update(model);
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int characterId, CancellationToken ct)
        {
            var entity = await _db.Set<CharacterModel>().FindAsync(new object?[] { characterId }, ct);
            if (entity != null)
            {
                _db.Remove(entity);
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
