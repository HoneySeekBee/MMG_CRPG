using Contracts.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Reader
{
    public interface IStageAssetReader
    {
        Task<StagePb?> GetStageAsync(int stageId, CancellationToken ct);
    }

}
