using Application.Combat.Engine;
using Application.Monsters;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Reader
{
    public sealed class MonsterStatReader : IMonsterStatReader
    {
        private readonly GameDBContext _db;

        public MonsterStatReader(GameDBContext db)
        {
            _db = db;
        }

        public async Task<MonsterStatDto?> GetAsync(long monsterId, int level, CancellationToken ct)
        {
            // MonsterStatProgression 에서 바로 조회
            var row = await _db.monsterStatProgressions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    s => s.MonsterId == monsterId && s.Level == level,
                    ct);

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
                // DB에는 50.00, 150.00 이런 식이므로 100으로 나눠서 비율로 변환
                CritRate =  (row.CritRate / 100m),  // 50.00 → 0.5
                CritDamage = (row.CritDamage / 100m),  // 150.00 → 1.5
                Range = row.Range
            };
        }
    }
} 
