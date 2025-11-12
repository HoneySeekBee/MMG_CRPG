using Domain.Entities;
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
        Task<Domain.Entities.GachaPool?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Domain.Entities.GachaPool?> GetWithEntriesAsync(int id, CancellationToken ct = default);

        // 검색/목록
        Task<(IReadOnlyList<Domain.Entities.GachaPool> Items, int Total)> SearchAsync(
            string? keyword = null, int skip = 0, int take = 20, CancellationToken ct = default);

        Task<IReadOnlyList<Domain.Entities.GachaPool>> ListAsync(int take = 100, CancellationToken ct = default);

        // CUD
        Task AddAsync(Domain.Entities.GachaPool pool, CancellationToken ct = default);
        Task UpdateAsync(Domain.Entities.GachaPool pool, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        // 확률표 전체 교체(벌크 업서트 용도)
        Task ReplaceEntriesAsync(int poolId, IEnumerable<GachaPoolEntry> entries, CancellationToken ct = default);

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
