using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gacha.GachaDraw
{
    public sealed record GachaDrawRequest(
        string BannerKey,
        int Count,
        int UserId
    );
}
