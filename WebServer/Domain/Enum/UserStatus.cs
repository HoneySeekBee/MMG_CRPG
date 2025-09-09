using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum UserStatus : short
    {
        Active = 0,
        Suspended = 1,
        Deleted = 2
    }

    public enum SecurityEventType : short
    {
        LoginSuccess = 0,
        LoginFail = 1,
        TokenRefresh = 2,
        Logout = 3
    }
}
