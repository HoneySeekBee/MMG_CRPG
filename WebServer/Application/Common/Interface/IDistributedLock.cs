using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interface
{
    public interface IDistributedLock
    {
        Task<bool> AcquireAsync(string key, TimeSpan expiry);
        Task ReleaseAsync(string key);
    }
}
