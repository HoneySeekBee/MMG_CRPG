using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gacha.GachaDraw
{
    public sealed record DrawResultDto(
     DateTimeOffset Timestamp,
     IReadOnlyList<DrawResultItemDto> Items,
     int UsedTickets,
     long UsedCurrency
 )
    {
        public int TotalCharacters => Items.Count(x => !x.IsShard);
        public int TotalShards => Items.Sum(x => x.ShardAmount);
    }
    public sealed record DrawResultItemDto(
      int CharacterId,
      int Grade,
      bool RateUp,
      bool IsNew,
      bool IsShard,
      int ShardAmount,
      bool IsGuaranteed
  );
}
