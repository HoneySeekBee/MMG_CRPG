using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Users.Caching
{
    public interface IUserCacheService
    {
        // ---- ProfileCore ----- 
        Task<UserProfileCoreCacheDto?> GetProfileCoreAsync(int userId, CancellationToken ct);
        Task SetProfileCoreAsync(int userId, UserProfileCoreCacheDto dto, CancellationToken ct);
        Task InvalidateProfileCoreAsync(int userId, CancellationToken ct);

        // ----- Wallet ---------
        Task<UserWalletCacheDto?> GetWalletAsync(int userId, CancellationToken ct);
        Task SetWalletAsync(int userId, UserWalletCacheDto dto, CancellationToken ct);
        Task InvalidateWalletAsync(int userId, CancellationToken ct);
         
        // ----- Convenience ------ 
        Task InvalidateUserAsync(int userId, CancellationToken ct); 
    }
}
