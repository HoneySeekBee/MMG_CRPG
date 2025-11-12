using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserInventory
{
    public sealed record UserInventoryListQuery(
        int UserId,                        // 필수
        int Page = 1,                      // 페이징
        int PageSize = 50,                 // 기본 페이지 크기
        int? ItemId = null,                // 특정 아이템만 필터
        DateTimeOffset? UpdatedFrom = null,
        DateTimeOffset? UpdatedTo = null
    );
    public sealed record ItemOwnershipQuery(
        int ItemId,
        int Page = 1,
        int PageSize = 50,
        int? MinCount = null               // 수량 조건
    );
    public sealed record GrantItemRequest(
        int UserId,
        int ItemId,
        int Amount                  // 지급/증가 수량
    );

    // 아이템 차감(소모)
    public sealed record ConsumeItemRequest(
        int UserId,
        int ItemId,
        int Amount                  // 소모 수량
    );

    // 아이템 수량 강제 설정(운영툴)
    public sealed record SetItemCountRequest(
        int UserId,
        int ItemId,
        int NewCount
    );

    // 인벤토리 아이템 삭제(Count=0이 아니라 행 자체 삭제)
    public sealed record DeleteItemRequest(
        int UserId,
        int ItemId
    );
}
