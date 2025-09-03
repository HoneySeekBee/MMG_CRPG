using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface ICharacterRepository
    {
        Task<(IReadOnlyList<Domain.Entities.Character> Items, int TotalCount)>
            GetPagedAsync(int page, int pageSize, int? elementId, int? rarityId, string? search, CancellationToken ct);

        Task<Domain.Entities.Character?> GetByIdAsync(int id, CancellationToken ct);

        /// <summary>자식 컬렉션(스킬/레벨/승급)까지 포함한 집합을 로드</summary>
        Task<(Domain.Entities.Character? Character,
              IReadOnlyList<CharacterSkill> Skills,
              IReadOnlyList<CharacterStatProgression> Progressions,
              IReadOnlyList<CharacterPromotion> Promotions)>
            GetAggregateAsync(int id, CancellationToken ct);

        Task AddAsync(Domain.Entities.Character entity, CancellationToken ct);
        Task RemoveAsync(Domain.Entities.Character entity, CancellationToken ct);

        // 대체(Replace) 방식 업서트 — 단순하고 버그 적음
        Task ReplaceSkillsAsync(int characterId, IEnumerable<CharacterSkill> skills, CancellationToken ct);
        Task ReplaceProgressionsAsync(int characterId, IEnumerable<CharacterStatProgression> progressions, CancellationToken ct);
        Task ReplacePromotionsAsync(int characterId, IEnumerable<CharacterPromotion> promotions, CancellationToken ct);

        Task SaveChangesAsync(CancellationToken ct);
    }
}
