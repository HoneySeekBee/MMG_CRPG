using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum GachaBannerStatus : short
    {
        Draft = 0,
        Scheduled = 1,
        Live = 2,
        Paused = 3,
        Archived = 4
    }
}
