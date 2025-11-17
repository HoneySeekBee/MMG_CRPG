using Domain.Entities.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.StageReward
{
    public interface IStageRewardService
    {
        Task<StageRewardResult> GrantRewardsAsync(
        int userId,
        int stageId,
        bool success,
        StageStars stars,
        DateTime nowUtc,
        CancellationToken ct = default);
    }
}
