using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CharacterModels
{
    public interface ICharacterModelService
    {
        Task<CharacterModelDto?> GetByCharacterIdAsync(int characterId, CancellationToken ct = default);
        Task<List<CharacterModelDto>> GetAllAsync(CancellationToken ct = default);
        Task<int> CreateAsync(CreateCharacterModelRequest req, CancellationToken ct = default);
        Task UpdateAsync(int characterId, CreateCharacterModelRequest req, CancellationToken ct = default);
        Task DeleteAsync(int characterId, CancellationToken ct = default);
    }
}
