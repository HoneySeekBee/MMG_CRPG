using Application.Combat.Engine;
using Application.Monsters;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Reader
{
    public sealed class EfMonsterStatReader : IMonsterStatReader
    {
        private readonly CombatDbContext _db;

        public EfMonsterStatReader(CombatDbContext db) => _db = db;

        public async Task<MonsterStatDto?> GetAsync(long monsterId, int level, CancellationToken ct)
        {
            var row = await _db.MonsterStatRows
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MonsterId == monsterId && s.Level == level, ct);

            if (row is null)
                return null;

            return new MonsterStatDto
            {
                MonsterId = row.MonsterId,
                Level = row.Level,
                HP = row.HP,
                ATK = row.ATK,
                DEF = row.DEF,
                SPD = row.SPD,
                CritRate = row.CritRate / 100m,
                CritDamage = row.CritDamage / 100m,
                Range = row.Range
            };
        }
    }
}
