using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Protos;

namespace Infrastructure.Reader
{
    public interface ICharacterAssetReader
    {
        Task<IReadOnlyDictionary<int, CharacterDetailPb>> GetCharactersAsync(
            IReadOnlyCollection<int> ids, CancellationToken ct);
    }
}
