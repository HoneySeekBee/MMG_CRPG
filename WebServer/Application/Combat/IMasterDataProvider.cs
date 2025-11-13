using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public interface IMasterDataProvider
    {
        // 전투 엔진 용 
        Task<Domain.Services.MasterDataPack> BuildEnginePackAsync(
        int stageId, 
        IReadOnlyCollection<long> partyCharacterIds,
        CancellationToken ct);

        // 클라이언트 초기 스냅샷 용 
        Task<MasterPackDto> BuildPackAsync(
            int stageId,
            long userid,
            IReadOnlyCollection<long> partyCharacterIds,  
            CancellationToken ct); 
    }

}
