using Application.Repositories;
using Application.UserInventory;
using Domain.Entities.User;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class UserInventoryQueryRepository : IUserInventoryQueryRepository
    {
        private readonly GameDBContext _db;
        public UserInventoryQueryRepository(GameDBContext db) => _db = db;

        // 유저별 인벤토리 페이지 조회
        public async Task<(IReadOnlyList<UserInventory> Rows, int Total)> GetPagedAsync(
            UserInventoryListQuery query,
            CancellationToken ct)
        {
            var q = _db.UserInventories.AsNoTracking().Where(x => x.UserId == query.UserId);

            if (query.ItemId is int itemId && itemId > 0)
                q = q.Where(x => x.ItemId == itemId);

            if (query.UpdatedFrom is { } from)
                q = q.Where(x => x.UpdatedAt >= from);

            if (query.UpdatedTo is { } to)
                q = q.Where(x => x.UpdatedAt <= to);

            var total = await q.CountAsync(ct);

            // 최근 갱신순으로
            var rows = await q.OrderByDescending(x => x.UpdatedAt)
                              .ThenBy(x => x.ItemId)
                              .Skip((query.Page - 1) * query.PageSize)
                              .Take(query.PageSize)
                              .ToListAsync(ct);

            return (rows, total);
        }

        // 특정 아이템을 가진 유저 역조회
        public async Task<(IReadOnlyList<UserInventory> Rows, int Total)> GetOwnersPagedAsync(
            ItemOwnershipQuery query,
            CancellationToken ct)
        {
            var q = _db.UserInventories.AsNoTracking().Where(x => x.ItemId == query.ItemId);

            if (query.MinCount is int min && min > 0)
                q = q.Where(x => x.Count >= min);

            var total = await q.CountAsync(ct);

            // 많이 가진 순 → 최신 갱신순
            var rows = await q.OrderByDescending(x => x.Count)
                              .ThenByDescending(x => x.UpdatedAt)
                              .Skip((query.Page - 1) * query.PageSize)
                              .Take(query.PageSize)
                              .ToListAsync(ct);

            return (rows, total);
        }
    }
}
