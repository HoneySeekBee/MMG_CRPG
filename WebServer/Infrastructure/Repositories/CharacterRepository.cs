using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class CharacterRepository : ICharacterRepository
    {
        private readonly GameDBContext _db;

        public CharacterRepository(GameDBContext db) => _db = db;

        // ===== Reads =====

        public async Task<(IReadOnlyList<Domain.Entities.Character> Items, int TotalCount)>
            GetPagedAsync(int page, int pageSize, int? elementId, int? rarityId, string? search, CancellationToken ct)
        {
            var query = _db.Characters.AsQueryable();

            if (elementId is { } e) query = query.Where(x => x.ElementId == e);
            if (rarityId is { } r) query = query.Where(x => x.RarityId == r);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                // Npgsql 사용 시 ILIKE로 대체 가능: EF.Functions.ILike(x.Name, $"%{s}%")
                query = query.Where(x => EF.Functions.Like(x.Name, $"%{s}%"));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            return (items, total);
        }

        public Task<Domain.Entities.Character?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.Characters.FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<(Domain.Entities.Character? Character,
                           IReadOnlyList<CharacterSkill> Skills,
                           IReadOnlyList<CharacterStatProgression> Progressions,
                           IReadOnlyList<CharacterPromotion> Promotions)>
            GetAggregateAsync(int id, CancellationToken ct)
        {
            var character = await _db.Characters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (character is null)
                return (null, Array.Empty<CharacterSkill>(), Array.Empty<CharacterStatProgression>(), Array.Empty<CharacterPromotion>());

            var skills = await _db.CharacterSkills
                .Where(x => x.CharacterId == id)
                .OrderBy(x => x.Slot)
                .AsNoTracking()
                .ToListAsync(ct);

            var progressions = await _db.CharacterStatProgressions
                .Where(x => x.CharacterId == id)
                .OrderBy(x => x.Level)
                .AsNoTracking()
                .ToListAsync(ct);

            var promotions = await _db.CharacterPromotions
                .Where(x => x.CharacterId == id)
                .OrderBy(x => x.Tier)
                .AsNoTracking()
                .ToListAsync(ct);

            return (character, skills, progressions, promotions);
        }

        // ===== Commands =====

        public async Task AddAsync(Domain.Entities.Character entity, CancellationToken ct)
            => await _db.Characters.AddAsync(entity, ct);

        public Task RemoveAsync(Domain.Entities.Character entity, CancellationToken ct)
        {
            _db.Characters.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task ReplaceSkillsAsync(int characterId, IEnumerable<CharacterSkill> skills, CancellationToken ct)
        {
            // 전체 교체: 기존 삭제 → 새 삽입
            var existing = await _db.CharacterSkills.Where(x => x.CharacterId == characterId).ToListAsync(ct);
            _db.CharacterSkills.RemoveRange(existing);

            // 안전 가드: CharacterId 강제 일치
            foreach (var s in skills) s.GetType().GetProperty(nameof(CharacterSkill.CharacterId))!
                                        .SetValue(s, characterId);

            await _db.CharacterSkills.AddRangeAsync(skills, ct);
        }

        public async Task ReplaceProgressionsAsync(int characterId, IEnumerable<CharacterStatProgression> progressions, CancellationToken ct)
        {
            var existing = await _db.CharacterStatProgressions.Where(x => x.CharacterId == characterId).ToListAsync(ct);
            _db.CharacterStatProgressions.RemoveRange(existing);

            foreach (var p in progressions) p.GetType().GetProperty(nameof(CharacterStatProgression.CharacterId))!
                                             .SetValue(p, characterId);

            await _db.CharacterStatProgressions.AddRangeAsync(progressions, ct);
        }

        public async Task ReplacePromotionsAsync(int characterId, IEnumerable<CharacterPromotion> promotions, CancellationToken ct)
        {
            var existing = await _db.CharacterPromotions.Where(x => x.CharacterId == characterId).ToListAsync(ct);
            _db.CharacterPromotions.RemoveRange(existing);

            foreach (var p in promotions) p.GetType().GetProperty(nameof(CharacterPromotion.CharacterId))!
                                           .SetValue(p, characterId);

            await _db.CharacterPromotions.AddRangeAsync(promotions, ct);
        }

        public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
