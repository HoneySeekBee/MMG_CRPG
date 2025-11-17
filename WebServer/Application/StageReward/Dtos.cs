using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.StageReward
{
    public sealed record GainedRewardDto(int ItemId, long Qty, bool FirstClearReward); 
    public sealed record StageRewardResult(int StageId, bool Success, bool IsFirstClear, IReadOnlyList<GainedRewardDto> Rewards, long Gold, long Gem, long Token);

}
