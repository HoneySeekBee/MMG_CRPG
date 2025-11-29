using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gacha.GachaPool
{
    public interface IGachaPoolService
    {
        Task<GachaPoolDto?> GetAsync(int poolId, CancellationToken ct = default);
        Task<GachaPoolDetailDto?> GetDetailAsync(int poolId, CancellationToken ct = default);

        Task<(IReadOnlyList<GachaPoolDto> Items, int Total)> SearchAsync(QueryGachaPoolsRequest req, CancellationToken ct = default);
        Task<IReadOnlyList<GachaPoolDto>> ListAsync(int take = 100, CancellationToken ct = default);

        Task<GachaPoolDto> CreateAsync(CreateGachaPoolRequest req, CancellationToken ct = default);
        Task<GachaPoolDto> UpdateAsync(UpdateGachaPoolRequest req, CancellationToken ct = default);
        Task DeleteAsync(int poolId, CancellationToken ct = default);

        Task ReplaceEntriesAsync(UpsertGachaPoolEntriesRequest req, CancellationToken ct = default);
    }
}
