using Application.Users;
using CommonModels = Application.Common.Models;

namespace AdminTool.Models
{
    public static class SecurityEventVmMappings
    {
        public static SecurityEventListVm ToVm(
            this CommonModels.PagedResult<SecurityEventBriefDto> page,  // 공용 PagedResult로 고정
            SecurityEventSearchVm q)
        {
            return new SecurityEventListVm
            {
                Search = q,
                Items = page.Items.Select(x => new SecurityEventItemVm
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Type = x.Type,
                    MetaJson = x.MetaJson,
                    CreatedAt = x.CreatedAt
                }).ToList(),
                Paging = new PaginationVm
                {
                    Page = page.Page,
                    PageSize = page.PageSize,
                    TotalCount = (int)page.TotalCount
                    // TotalPages/HasPrev/HasNext 는 계산 속성이라 자동
                }
            };
        }
    }
}