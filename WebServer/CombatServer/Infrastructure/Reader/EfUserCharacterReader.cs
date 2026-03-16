using Application.UserCharacter;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Reader
{
    public sealed class EfUserCharacterReader : IUserCharacterReader
    {
        private readonly CombatDbContext _db;

        public EfUserCharacterReader(CombatDbContext db) => _db = db;

        public async Task<UserCharacterStatsDto?> GetAsync(long userCharacterId, CancellationToken ct)
        {
            var list = await GetManyAsync(new[] { userCharacterId }, ct);
            return list.FirstOrDefault();
        }

        public async Task<IReadOnlyList<UserCharacterStatsDto>> GetManyAsync(
            IReadOnlyCollection<long> userCharacterIds, CancellationToken ct)
        {
            if (userCharacterIds.Count == 0)
                return Array.Empty<UserCharacterStatsDto>();

            var query =
                from uc in _db.UserCharacterRows.AsNoTracking()
                join stat in _db.CharacterStatRows.AsNoTracking()
                    on new { uc.CharacterId, Level = uc.Level }
                    equals new { CharacterId = (int)stat.CharacterId, stat.Level }
                where userCharacterIds.Contains(uc.UserCharacterId)
                select new UserCharacterStatsDto(
                    uc.UserCharacterId,
                    uc.UserId,
                    uc.CharacterId,
                    uc.Level,
                    stat.HP,
                    stat.ATK,
                    stat.DEF,
                    stat.SPD,
                    (double)stat.CriRate / 100.0,
                    (double)stat.CriDamage / 100.0,
                    stat.Range
                );

            return await query.ToListAsync(ct);
        }

        public async Task<IReadOnlyList<UserCharacterStatsDto>> GetManyByCharacterIdAsync(
            IReadOnlyCollection<long> characterIds, long userId, CancellationToken ct)
        {
            if (characterIds.Count == 0)
                return Array.Empty<UserCharacterStatsDto>();

            var query =
                from uc in _db.UserCharacterRows.AsNoTracking()
                join stat in _db.CharacterStatRows.AsNoTracking()
                    on new { uc.CharacterId, Level = uc.Level }
                    equals new { CharacterId = (int)stat.CharacterId, stat.Level }
                where characterIds.Contains(uc.CharacterId) && uc.UserId == userId
                select new UserCharacterStatsDto(
                    uc.UserCharacterId,
                    uc.UserId,
                    uc.CharacterId,
                    uc.Level,
                    stat.HP,
                    stat.ATK,
                    stat.DEF,
                    stat.SPD,
                    (double)stat.CriRate / 100.0,
                    (double)stat.CriDamage / 100.0,
                    stat.Range
                );

            return await query.ToListAsync(ct);
        }
    }
}
