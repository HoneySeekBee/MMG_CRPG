using Application.Gacha.GachaBanner;
using Application.Gacha.GachaPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gacha
{
    public sealed record CachedGachaCatalogDto(
        IReadOnlyList<GachaBannerDto> Banners,
        IReadOnlyList<GachaPoolDetailDto> Pools,
        long Version
    );
}
