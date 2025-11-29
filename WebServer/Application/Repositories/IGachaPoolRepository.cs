using Domain.Entities.Gacha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IGachaPoolRepository
    {
        // 기본 단건
        Task<GachaPool?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<GachaPool?> GetWithEntriesAsync(int id, CancellationToken ct = default);

        // 검색/목록
        Task<(IReadOnlyList<GachaPool> Items, int Total)> SearchAsync(
            string? keyword = null, int skip = 0, int take = 20, CancellationToken ct = default);

        Task<IReadOnlyList<GachaPool>> ListAsync(int take = 100, CancellationToken ct = default);

        // CUD
        Task AddAsync(GachaPool pool, CancellationToken ct = default);
        Task UpdateAsync(GachaPool pool, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        // 확률표 전체 교체(벌크 업서트 용도)
        Task ReplaceEntriesAsync(int poolId, IEnumerable<GachaPoolEntry> entries, CancellationToken ct = default);

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
