using Application.Combat;
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
    public sealed class EfCharacterReader : ICharacterReader
    {
        private readonly GameDBContext _db;

        public EfCharacterReader(GameDBContext db)
        {
            _db = db;
        }
        public async Task<CharacterMasterDto> GetAsync(long characterId, CancellationToken ct)
        {
            // 1) 캐릭터 존재 여부만 확인 (이름/속성용)
            var c = await _db.Characters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == characterId, ct);

            if (c is null)
                throw new KeyNotFoundException($"Character {characterId} not found");

            // 2) 레벨 1 기준 스탯을 가져온다 (원하면 다른 레벨도 가능)
            var stat = await _db.CharacterStatProgressions
                .AsNoTracking()
                .Where(p => p.CharacterId == characterId && p.Level == 1)
                .FirstOrDefaultAsync(ct);

            if (stat is null)
                throw new KeyNotFoundException($"CharacterStatProgression not found for character {characterId} level 1");

            // 3) progression 값으로 CharacterMasterDto 구성
            return new CharacterMasterDto(
                CharacterId: c.Id,
                BaseHp: stat.HP,
                BaseAtk: stat.ATK,
                BaseDef: stat.DEF,
                BaseAspd: stat.SPD
            );
        }
    }
}