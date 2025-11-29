using Application.Contents.Stages;
using Application.Gacha.GachaBanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gacha.GachaDraw
{
    public interface IGachaDrawService
    {
        Task<DrawResultDto> DrawAsync(string bannerKey, int count, int userId, CancellationToken ct); 
    }
}
