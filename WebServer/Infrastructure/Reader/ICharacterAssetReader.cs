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
        Task<IReadOnlyDictionary<long, CharacterDetailPb>> GetCharactersAsync(
            IReadOnlyCollection<long> ids, CancellationToken ct);
    }
}
