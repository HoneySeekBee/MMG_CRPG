using Application.Combat;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Reader
{
    public sealed class EfCharacterReader : ICharacterReader
    {
        private readonly CombatDbContext _db;

        public EfCharacterReader(CombatDbContext db) => _db = db;

        public async Task<CharacterMasterDto?> GetAsync(long characterId, CancellationToken ct)
        {
            var stat = await _db.CharacterStatRows
                .Where(s => s.CharacterId == characterId && s.Level == 1)
                .FirstOrDefaultAsync(ct);

            if (stat is null)
                return null;

            return new CharacterMasterDto(
                CharacterId: characterId,
                BaseHp: stat.HP,
                BaseAtk: stat.ATK,
                BaseDef: stat.DEF,
                BaseAspd: stat.SPD
            );
        }
    }
}
