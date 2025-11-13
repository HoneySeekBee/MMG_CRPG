using Application.UserCharacter;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Reader
{
    public class EfUserCharacterReader : IUserCharacterReader
    {
        private readonly GameDBContext _db;

        public EfUserCharacterReader(GameDBContext db)
        {
            _db = db;
        }

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
                from uc in _db.UserCharacters.AsNoTracking()
                join prog in _db.CharacterStatProgressions.AsNoTracking()
                    on new { uc.CharacterId, Level = (short)uc.Level }
                    equals new { prog.CharacterId, prog.Level }
                where userCharacterIds.Contains(uc.UserCharacterId)
                select new UserCharacterStatsDto(
                    uc.UserCharacterId,
                    uc.UserId,
                    uc.CharacterId,
                    (short)uc.Level,
                    prog.HP,
                    prog.ATK,
                    prog.DEF,
                    prog.SPD,
                    (double)prog.CriRate / 100.0,      // 5m → 0.05
                    (double)prog.CriDamage / 100.0,    // 150m → 1.5
                    prog.Range
                );

            return await query.ToListAsync(ct);
        }

        public async Task<IReadOnlyList<UserCharacterStatsDto>> GetManyByCharacterIdAsync(
    IReadOnlyCollection<long> characterIds,
    long userId,
    CancellationToken ct)
        {
            var query =
                from uc in _db.UserCharacters.AsNoTracking()
                join prog in _db.CharacterStatProgressions.AsNoTracking()
                    on new { uc.CharacterId, Level = (short)uc.Level }
                    equals new { prog.CharacterId, prog.Level }
                where characterIds.Contains(uc.CharacterId) && uc.UserId == userId
                select new UserCharacterStatsDto(
                    uc.UserCharacterId,
                    uc.UserId,
                    uc.CharacterId,
                    (short)uc.Level,
                    prog.HP,
                    prog.ATK,
                    prog.DEF,
                    prog.SPD,
                    (double)prog.CriRate / 100.0,
                    (double)prog.CriDamage / 100.0,
                    prog.Range
                );

            return await query.ToListAsync(ct);
        }

    }
}
