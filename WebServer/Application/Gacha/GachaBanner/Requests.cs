using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gacha.GachaBanner
{
    public record CreateGachaBannerRequest(
       string Key,
       string Title,
       int GachaPoolId,
       DateTimeOffset? StartsAt,
       DateTimeOffset? EndsAt,
       string? Subtitle,
       int? PortraitId,
       int CostCurrencyId,
       int Cost,
       int TicketItemId,
       short Priority = 0,
       GachaBannerStatus Status = GachaBannerStatus.Live,
       bool IsActive = true
   );
    public record UpdateGachaBannerRequest(
    int Id,
    string Title,
    string? Subtitle,
    int? PortraitId,
    int GachaPoolId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    int CostCurrencyId,
    int Cost,
    int TicketItemId,
    short Priority,
    GachaBannerStatus Status,
    bool IsActive
);
    public record QueryGachaBannersRequest(
        string? Keyword = null,
        int Skip = 0,
        int Take = 20
    );
}
