using System;
using System.Linq;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes; 
using Application.UserInventory;
using Application.Common.Models;
using Contracts.Protos;

namespace WebServer.Mappers
{
    public static class UserInventoryProtoMapping
    {
        public static UserInventory ToPb(this UserInventoryDto dto)
        => new UserInventory
        {
            UserId = dto.UserId,
            ItemId = dto.ItemId,
            Count = dto.Count,
            // DateTimeOffset → Timestamp (UTC)
            UpdatedAt = Timestamp.FromDateTime(dto.UpdatedAt.UtcDateTime)
        };
        public static ListUserInventoryResponse ToPb(this PagedResult<UserInventoryDto> paged)
        {
            var resp = new ListUserInventoryResponse
            {
                PageInfo = new PageInfo
                {
                    Page = paged.Page,
                    PageSize = paged.PageSize,
                    TotalCount = (int)paged.TotalCount
                }
            };

            resp.Items.AddRange(paged.Items.Select(i => i.ToPb()));
            return resp;
        }

        public static GetUserInventoryResponse ToPbGet(this UserInventoryDto dto)
        => new GetUserInventoryResponse { Item = dto.ToPb() };

        public static GrantItemResponse ToPbGrant(this UserInventoryDto dto)
      => new GrantItemResponse { Item = dto.ToPb() };

        public static SetItemCountResponse ToPbSet(this UserInventoryDto dto)
      => new SetItemCountResponse { Item = dto.ToPb() };

        public static ConsumeItemResponse ToPb(this IUserInventoryService.ConsumeResultDto r)
     => new ConsumeItemResponse
     {
         Success = r.Success,
         BeforeCount = 0,       // 서비스에서 제공 안 함 → 기본값
         AfterCount = r.CurrentCount,
         Consumed = 0,          // 계산 불가 → 기본값
         FailureReason = r.Success ? "" : "Insufficient items" // 실패 원인 하드코딩 or 정책적으로 정리
     };

        public static PageInfo ToPbPageInfo(int page, int pageSize, int totalCount)
       => new PageInfo { Page = page, PageSize = pageSize, TotalCount = totalCount };

    }
}
