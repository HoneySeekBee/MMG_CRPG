using Application.GachaBanner;
using Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public sealed class GachaBannerListItemVm
    {
        public int Id { get; init; }
        public string Key { get; init; } = default!;
        public string Title { get; init; } = default!;
        public string? Subtitle { get; init; }
        public int GachaPoolId { get; init; }
        public short Priority { get; init; }
        public GachaBannerStatus Status { get; init; }
        public bool IsActive { get; init; }
        public DateTimeOffset StartsAt { get; init; }
        public DateTimeOffset? EndsAt { get; init; }
        public bool IsLiveNow { get; init; }

        public static GachaBannerListItemVm FromDto(GachaBannerDto d) => new()
        {
            Id = d.Id,
            Key = d.Key,
            Title = d.Title,
            Subtitle = d.Subtitle,
            GachaPoolId = d.GachaPoolId,
            Priority = d.Priority,
            Status = d.Status,
            IsActive = d.IsActive,
            StartsAt = d.StartsAt,
            EndsAt = d.EndsAt,
            IsLiveNow = d.IsLiveNow
        };
    }

    public sealed class GachaBannerIndexVm
    {
        public IReadOnlyList<GachaBannerListItemVm> Items { get; init; } = Array.Empty<GachaBannerListItemVm>();
        public int Total { get; init; }
        public int Skip { get; init; }
        public int Take { get; init; } = 20;

        public GachaBannerFilterVm Filter { get; init; } = new();
    }

    public sealed class GachaBannerFilterVm
    {
        [Display(Name = "검색어")]
        public string? Keyword { get; set; }

        [Display(Name = "상태")]
        public GachaBannerStatus? Status { get; set; }

        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 20;
    }
}
