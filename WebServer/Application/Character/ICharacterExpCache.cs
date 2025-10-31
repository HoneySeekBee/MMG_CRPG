using Domain.Entities.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Character
{
    public interface ICharacterExpCache
    {
        IReadOnlyList<CharacterExp> GetAll();
        IReadOnlyList<CharacterExp> GetByRarity(int rarityId);
        CharacterExp? Get(int rarityId, short level);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
