using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Character
{
    public interface ICharacterService
    {
        Task<PagedResult<CharacterSummaryDto>> GetListAsync(CharacterListQuery query, CancellationToken ct);
        Task<CharacterDetailDto?> GetDetailAsync(int characterId, CancellationToken ct);

        Task<int> CreateAsync(CreateCharacterRequest request, CancellationToken ct);
        Task UpdateBasicAsync(int characterId, UpdateCharacterRequest request, CancellationToken ct);

        Task SetSkillsAsync(int characterId, IReadOnlyList<UpsertSkillRequest> skills, CancellationToken ct);
        Task SetProgressionsAsync(int characterId, IReadOnlyList<UpsertProgressionRequest> progressions, CancellationToken ct);
        Task SetPromotionsAsync(int characterId, IReadOnlyList<UpsertPromotionRequest> promotions, CancellationToken ct);

        Task DeleteAsync(int characterId, CancellationToken ct);
    }
}
