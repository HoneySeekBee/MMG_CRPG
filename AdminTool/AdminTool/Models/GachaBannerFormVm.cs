using Application.GachaBanner;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public sealed class GachaBannerFormVm : IValidatableObject
    {// Edit일 때만 값 존재
        public int? Id { get; set; }

        [Required, StringLength(120)]
        [Display(Name = "Key (고유)")]
        public string Key { get; set; } = default!;

        [Required, StringLength(120)]
        [Display(Name = "제목")]
        public string Title { get; set; } = default!;

        [StringLength(200)]
        [Display(Name = "부제목")]
        public string? Subtitle { get; set; }

        [Display(Name = "Portrait Id (이미지)")]
        public int? PortraitId { get; set; }

        [Required]
        [Display(Name = "가차 풀")]
        public int GachaPoolId { get; set; }

        // 운영툴은 보통 “로컬 시간”으로 입력 → 서비스에서 UTC로 변환
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "시작(로컬)")]
        public DateTime? StartsAtLocal { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "종료(로컬)")]
        public DateTime? EndsAtLocal { get; set; }

        [Display(Name = "우선순위"), Range(short.MinValue, short.MaxValue)]
        public short Priority { get; set; } = 0;

        [Display(Name = "상태")]
        public GachaBannerStatus Status { get; set; } = GachaBannerStatus.Live;

        [Display(Name = "활성화")]
        public bool IsActive { get; set; } = true;

        // 드롭다운 소스
        public IEnumerable<SelectListItem>? PoolOptions { get; set; }
        public IEnumerable<SelectListItem>? PortraitOptions { get; set; }

        // 폼 유효성 (종료 > 시작)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartsAtLocal is null)
                yield return new ValidationResult("시작 시간은 필수입니다.", new[] { nameof(StartsAtLocal) });

            if (StartsAtLocal is not null && EndsAtLocal is not null && EndsAtLocal <= StartsAtLocal)
                yield return new ValidationResult("종료 시간은 시작 시간보다 커야 합니다.", new[] { nameof(EndsAtLocal) });
        }

        // ── 매핑 : VM → Request (로컬시간을 특정 타임존 기준 UTC로 변환)
        public CreateGachaBannerRequest ToCreateRequest(string timeZoneId)
        {
            var (startUtc, endUtc) = ToUtc(timeZoneId);
            return new CreateGachaBannerRequest(
                Key: Key,
                Title: Title,
                GachaPoolId: GachaPoolId,
                StartsAt: startUtc,
                EndsAt: endUtc,
                Subtitle: Subtitle,
                PortraitId: PortraitId,
                Priority: Priority,
                Status: Status,
                IsActive: IsActive
            );
        }

        public UpdateGachaBannerRequest ToUpdateRequest(string timeZoneId)
        {
            if (Id is null) throw new InvalidOperationException("Id is required for update");

            var (startUtc, endUtc) = ToUtc(timeZoneId);
            return new UpdateGachaBannerRequest(
                Id: Id.Value,
                Title: Title,
                Subtitle: Subtitle,
                PortraitId: PortraitId,
                GachaPoolId: GachaPoolId,
                StartsAt: startUtc ?? DateTimeOffset.UtcNow, // Required
                EndsAt: endUtc,
                Priority: Priority,
                Status: Status,
                IsActive: IsActive
            );
        }

        private (DateTimeOffset? startUtc, DateTimeOffset? endUtc) ToUtc(string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId)) timeZoneId = TimeZoneInfo.Local.Id;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            DateTimeOffset? startUtc = null, endUtc = null;
            if (StartsAtLocal is DateTime s)
                startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(s, DateTimeKind.Unspecified), tz);
            if (EndsAtLocal is DateTime e)
                endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(e, DateTimeKind.Unspecified), tz);

            return (startUtc, endUtc);
        }

        // ── 매핑 : DTO → VM (UTC → 로컬)
        public static GachaBannerFormVm FromDto(GachaBannerDto dto, string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId)) timeZoneId = TimeZoneInfo.Local.Id;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var startLocal = TimeZoneInfo.ConvertTime(dto.StartsAt.UtcDateTime, tz);
            DateTime? endLocal = dto.EndsAt.HasValue
                ? TimeZoneInfo.ConvertTime(dto.EndsAt.Value.UtcDateTime, tz)
                : null;

            return new GachaBannerFormVm
            {
                Id = dto.Id,
                Key = dto.Key,
                Title = dto.Title,
                Subtitle = dto.Subtitle,
                PortraitId = dto.PortraitId,
                GachaPoolId = dto.GachaPoolId,
                StartsAtLocal = startLocal,
                EndsAtLocal = endLocal,
                Priority = dto.Priority,
                Status = dto.Status,
                IsActive = dto.IsActive
            };
        }
    }
}
