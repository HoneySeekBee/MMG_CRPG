using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gacha.GachaBanner
{
    public record GachaBannerDto(
      int Id,
      string Key,
      string Title,
      string? Subtitle,
      int? PortraitId,
      int GachaPoolId,
      DateTimeOffset StartsAt,
      DateTimeOffset? EndsAt,
      short Priority,
      GachaBannerStatus Status,
      bool IsActive,
      bool IsLiveNow,
      int CostCurrencyId,
      int Cost,
      int TicketItemId
  );


    public static class GachaBannerMappings
    {
        public static GachaBannerDto ToDto(this Domain.Entities.Gacha.GachaBanner e, DateTimeOffset? now = null)
             => new(
        e.Id,
        e.Key,
        e.Title,
        e.Subtitle,
        e.PortraitId,
        e.GachaPoolId,
        e.StartsAt,
        e.EndsAt,
        e.Priority,
        e.Status,
        e.IsActive,
        e.IsLiveNow(now),
        e.CostCurrencyId,
        e.Cost,
        e.TicketItemId
    );
    }
}
