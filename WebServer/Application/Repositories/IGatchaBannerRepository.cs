using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Repositories
{
    public interface IGachaBannerRepository
    {
        Task<Domain.Entities.GachaBanner?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Domain.Entities.GachaBanner?> GetByKeyAsync(string key, CancellationToken ct = default);

        Task<IReadOnlyList<Domain.Entities.GachaBanner>> ListLiveAsync(DateTimeOffset? now = null, int take = 10, CancellationToken ct = default);
        Task<(IReadOnlyList<Domain.Entities.GachaBanner> Items, int Total)> SearchAsync(
            string? keyword = null, int skip = 0, int take = 20, CancellationToken ct = default);

        Task AddAsync(Domain.Entities.GachaBanner banner, CancellationToken ct = default);
        Task UpdateAsync(Domain.Entities.GachaBanner banner, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
