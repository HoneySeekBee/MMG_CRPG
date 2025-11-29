using Application.Gacha.GachaBanner;
using Application.Gacha.GachaPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gacha
{
    public interface IGachaCacheService
    {
        Task<IReadOnlyList<GachaBannerDto>> GetActiveBannersAsync(CancellationToken ct);
        Task<GachaPoolDetailDto?> GetPoolAsync(int poolId, CancellationToken ct); 
        Task RefreshAllAsync(CancellationToken ct);
    }
}
