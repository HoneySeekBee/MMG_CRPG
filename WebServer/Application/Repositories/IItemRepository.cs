using Application.Items;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IItemRepository
    {
        // 조회
        Task<Item?> GetByIdAsync(long id, bool includeChildren, CancellationToken ct);
        Task<Item?> GetByCodeAsync(string code, bool includeChildren, CancellationToken ct);

        Task<(IReadOnlyList<Item> Items, long TotalCount)> SearchAsync(
            ListItemsRequest req,
            CancellationToken ct);

        // 생성/삭제
        Task AddAsync(Item item, CancellationToken ct);
        Task DeleteAsync(Item item, CancellationToken ct);

        // 유효성 보조
        Task<bool> IsCodeUniqueAsync(string code, long? excludeId, CancellationToken ct);

        // 저장 (Unit of Work 없으면 여기서 처리)
        Task<int> SaveChangesAsync(CancellationToken ct);
    }

}
