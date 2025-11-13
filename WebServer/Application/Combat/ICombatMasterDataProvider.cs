using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public interface ICombatMasterDataProvider
    { 
        Task<CombatMasterDataPack> BuildPackAsync(
            int stageId,
            IReadOnlyCollection<long> playerCharacterIds,
            CancellationToken ct);
    }
}
