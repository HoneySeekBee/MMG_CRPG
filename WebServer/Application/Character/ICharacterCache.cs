using Application.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Character
{
    public interface ICharacterCache
    {
        IReadOnlyList<CharacterDetailDto> GetAll();
        CharacterDetailDto? GetById(int id);
        Task ReloadAsync(CancellationToken ct = default);
    }
}
