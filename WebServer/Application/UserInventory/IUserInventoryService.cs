using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Common.Models;

namespace Application.UserInventory
{
    public interface IUserInventoryService
    {
        // [1] 조회 

        // 유저의 인벤토리 목록 조회하기 
        Task<PagedResult<UserInventoryDto>> GetListAsync(
            UserInventoryListQuery query,
            CancellationToken ct);

        // 단일 항목 조회하기 
        Task<UserInventoryDto?> GetOneAsync(
            int userId,
            int itemId,
            CancellationToken ct);

        // 특정 아이템을 보유한 유저 목록 조회하기 
        Task<PagedResult<UserInventoryDto>> GetOwnersAsync(
            ItemOwnershipQuery query,
            CancellationToken ct);

        // [2] 작업

        // 아이템 지급/증가 
        Task<UserInventoryDto> GrantAsync(GrantItemRequest req, CancellationToken ct);

        // 아이템 소모
        Task<ConsumeResultDto> ConsumeAsync(
          ConsumeItemRequest req,
          CancellationToken ct);

        // 수량 강제 설정 
        Task<UserInventoryDto> SetCountAsync(SetItemCountRequest req, CancellationToken ct);

        // 항목 삭제 
        Task DeleteAsync(DeleteItemRequest req, CancellationToken ct);


        public sealed record ConsumeResultDto(
       bool Success,
       int CurrentCount
    );
    }

}
