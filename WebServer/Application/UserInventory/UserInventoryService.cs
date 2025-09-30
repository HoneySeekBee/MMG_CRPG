using Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.UserInventory.IUserInventoryService;
using UserInvenPagedResult = Application.Common.Models.PagedResult<Application.UserInventory.UserInventoryDto>;
using UserInven = Domain.Entities.User.UserInventory;

namespace Application.UserInventory
{
    public sealed class UserInventoryService : IUserInventoryService
    {
        private readonly IUserInventoryRepository _inv;          // write
        private readonly IUserInventoryQueryRepository _invQuery; // read
        private readonly IClock _clock;

        public UserInventoryService(
           IUserInventoryRepository inv,
           IUserInventoryQueryRepository invQuery,
           IClock clock)
        {
            _inv = inv;
            _invQuery = invQuery;
            _clock = clock;
        }

        public async Task<UserInvenPagedResult> GetListAsync(UserInventoryListQuery query, CancellationToken ct)
        {
            if (query.UserId <= 0) throw new ArgumentOutOfRangeException(nameof(query.UserId));
            if (query.Page <= 0) throw new ArgumentOutOfRangeException(nameof(query.Page));
            if (query.PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(query.PageSize));

            var (rows, total) = await _invQuery.GetPagedAsync(query, ct);


            var items = rows.Select(e => new UserInventoryDto(e.UserId, e.ItemId, e.Count, e.UpdatedAt)).ToList();
            return new UserInvenPagedResult(items, total, query.Page, query.PageSize);
        }
        public async Task<UserInventoryDto?> GetOneAsync(int userId, int itemId, CancellationToken ct)
        {
            if (userId <= 0 || itemId <= 0) throw new ArgumentOutOfRangeException();

            var e = await _inv.GetByKeyAsync(userId, itemId, ct);
            if (e is null) return null;

            return new UserInventoryDto(e.UserId, e.ItemId, e.Count, e.UpdatedAt);
        }
        public async Task<UserInvenPagedResult> GetOwnersAsync(ItemOwnershipQuery query, CancellationToken ct)
        {
            if (query.ItemId <= 0) throw new ArgumentOutOfRangeException(nameof(query.ItemId));
            if (query.Page <= 0) throw new ArgumentOutOfRangeException(nameof(query.Page));
            if (query.PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(query.PageSize));

            var (rows, total) = await _invQuery.GetOwnersPagedAsync(query, ct);
            var items = rows.Select(e => new UserInventoryDto(e.UserId, e.ItemId, e.Count, e.UpdatedAt)).ToList();
            return new UserInvenPagedResult(items, total, query.Page, query.PageSize);
        }
        public async Task<UserInventoryDto> GrantAsync(GrantItemRequest req, CancellationToken ct)
        {
            if (req.UserId <= 0 || req.ItemId <= 0) throw new ArgumentOutOfRangeException();
            if (req.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(req.Amount));

            var now = _clock.UtcNow;

            // 1) 조회
            var e = await _inv.GetByKeyAsync(req.UserId, req.ItemId, ct);

            if (e is null)
            {
                // 2-a) 없으면 생성
                e = UserInven.Create(req.UserId, req.ItemId, count: req.Amount, now);
                await _inv.AddAsync(e, ct);
            }
            else
            {
                // 2-b) 있으면 증가
                e.Add(req.Amount, now);
            }

            // 3) 저장
            await _inv.SaveChangesAsync(ct);

            return new UserInventoryDto(e.UserId, e.ItemId, e.Count, e.UpdatedAt);
        }

        public async Task<ConsumeResultDto> ConsumeAsync(ConsumeItemRequest req, CancellationToken ct)
        {
            if (req.UserId <= 0 || req.ItemId <= 0) throw new ArgumentOutOfRangeException();
            if (req.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(req.Amount));

            var e = await _inv.GetByKeyAsync(req.UserId, req.ItemId, ct);
            if (e is null)
            {
                return new ConsumeResultDto(Success: false, CurrentCount: 0);
            }

            var ok = e.TryConsume(req.Amount, _clock.UtcNow);
            if (!ok)
            {
                return new ConsumeResultDto(Success: false, CurrentCount: e.Count);
            }

            // 정책: 0이 되더라도 행 유지(삭제는 명령적으로만)
            await _inv.SaveChangesAsync(ct);
            return new ConsumeResultDto(Success: true, CurrentCount: e.Count);
        }

        public async Task<UserInventoryDto> SetCountAsync(SetItemCountRequest req, CancellationToken ct)
        {
            if (req.UserId <= 0 || req.ItemId <= 0) throw new ArgumentOutOfRangeException();
            if (req.NewCount < 0) throw new ArgumentOutOfRangeException(nameof(req.NewCount));

            var now = _clock.UtcNow;
            var e = await _inv.GetByKeyAsync(req.UserId, req.ItemId, ct);

            if (e is null)
            {
                e = UserInven.Create(req.UserId, req.ItemId, req.NewCount, now);
                await _inv.AddAsync(e, ct);
            }
            else
            {
                e.SetCount(req.NewCount, now);
            }

            await _inv.SaveChangesAsync(ct);
            return new UserInventoryDto(e.UserId, e.ItemId, e.Count, e.UpdatedAt);
        }

        public async Task DeleteAsync(DeleteItemRequest req, CancellationToken ct)
        {
            if (req.UserId <= 0 || req.ItemId <= 0) throw new ArgumentOutOfRangeException();

            var e = await _inv.GetByKeyAsync(req.UserId, req.ItemId, ct);
            if (e is null) return;

            _inv.Remove(e);
            await _inv.SaveChangesAsync(ct);
        }


    }
}
