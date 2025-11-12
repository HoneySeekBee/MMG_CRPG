using Application.Character;
using Domain.Entities.Characters;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public class CharacterExpCache : ICharacterExpCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        public CharacterExpCache(IDbContextFactory<GameDBContext> factory) => _factory = factory;

        // 전체 리스트 (정렬된 상태)
        private List<CharacterExp> _all = new();

        // 레어도 → 정렬된 레벨 리스트
        private Dictionary<short, List<CharacterExp>> _byRarity = new();

        // (레어도, 레벨) → 단건
        private Dictionary<(short rarityId, short level), CharacterExp> _byKey = new();

        private readonly object _gate = new();

        public IReadOnlyList<CharacterExp> GetAll()
        {
            lock (_gate) return _all;
        }

        public IReadOnlyList<CharacterExp> GetByRarity(int rarityId)
        {
            lock (_gate)
            {
                return _byRarity.TryGetValue((short)rarityId, out var list)
                    ? list
                    : Array.Empty<CharacterExp>();
            }
        }

        public CharacterExp? Get(int rarityId, short level)
        {
            lock (_gate)
            {
                return _byKey.TryGetValue(((short)rarityId, level), out var row) ? row : null;
            }
        }

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            // 한 방 로드 + 정렬
            var rows = await db.CharacterExps
                .AsNoTracking()
                .OrderBy(r => r.RarityId)
                .ThenBy(r => r.Level)
                .ToListAsync(ct);

            // 새 인덱스 구성
            var newAll = rows;
            var newByRarity = rows
                .GroupBy(r => r.RarityId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Level).ToList());
            var newByKey = rows.ToDictionary(r => (r.RarityId, r.Level));

            Console.WriteLine($"[ CharacterExp Cache ] {newAll.Count}");
            // 원자적 스왑
            lock (_gate)
            {
                _all = newAll;
                _byRarity = newByRarity;
                _byKey = newByKey;
            }
        }
    }
} 
