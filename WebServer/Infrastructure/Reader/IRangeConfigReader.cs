using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Reader
{
    public interface IRangeConfigReader
    { 
        Task<float?> GetRangeAsync(int masterId, bool isPlayer, CancellationToken ct);
    }
}
