using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Gacha
{
    public sealed class GachaDrawLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BannerId { get; set; }
        public int PoolId { get; set; }

        public string ResultsJson { get; set; } = string.Empty;

        public DateTimeOffset Timestamp { get; set; }
    }
}
