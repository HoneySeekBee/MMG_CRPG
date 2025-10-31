using Domain.Entities.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface ICharacterModelRepository
    {
        Task<CharacterModel?> GetByCharacterIdAsync(int characterId, CancellationToken ct = default);
        Task<List<CharacterModel>> GetAllAsync(CancellationToken ct = default);
        Task<CharacterModel> AddAsync(CharacterModel model, CancellationToken ct = default);
        Task UpdateAsync(CharacterModel model, CancellationToken ct = default);
        Task DeleteAsync(int characterId, CancellationToken ct = default);
    }
}
