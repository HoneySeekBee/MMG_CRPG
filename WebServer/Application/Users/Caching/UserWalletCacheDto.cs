using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Users.Caching
{
    public sealed record UserWalletCacheDto(
        int Gold,
        int Gem,
        int Token
    );
}
