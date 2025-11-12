using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public interface IMasterDataProvider
    {
        Task<Domain.Services.MasterDataPack> BuildPackAsync(
            int stageId,
            IReadOnlyCollection<long> partyCharacterIds,
            CancellationToken ct);
    }

}
