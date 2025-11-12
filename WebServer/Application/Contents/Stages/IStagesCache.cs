using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Stages
{
    public interface IStagesCache
    {
        IReadOnlyList<StageDetailDto> GetAll();
        StageDetailDto? GetById(int id);

        // DB에서 다시 긁어올 때
        Task ReloadAsync(CancellationToken ct = default);
    }
}
