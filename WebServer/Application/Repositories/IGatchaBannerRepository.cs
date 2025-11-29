using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Gacha;

namespace Application.Repositories
{
    public interface IGachaBannerRepository
    {
        Task<GachaBanner?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<GachaBanner?> GetByKeyAsync(string key, CancellationToken ct = default);

        Task<IReadOnlyList<GachaBanner>> ListLiveAsync(DateTimeOffset? now = null, int take = 10, CancellationToken ct = default);
        Task<(IReadOnlyList<GachaBanner> Items, int Total)> SearchAsync(
            string? keyword = null, int skip = 0, int take = 20, CancellationToken ct = default);

        Task AddAsync(GachaBanner banner, CancellationToken ct = default);
        Task UpdateAsync(GachaBanner banner, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
