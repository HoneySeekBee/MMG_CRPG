using Application.GachaPool;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public sealed class GachaPoolFilterVm
    {
        [Display(Name = "검색어")]
        public string? Keyword { get; set; }

        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 20;
    }

    public sealed class GachaPoolListItemVm
    {
        public int PoolId { get; init; }
        public string Name { get; init; } = string.Empty;
        public DateTimeOffset ScheduleStart { get; init; }
        public DateTimeOffset? ScheduleEnd { get; init; }
        public string? TablesVersion { get; init; }

        public static GachaPoolListItemVm FromDto(GachaPoolDto d) => new()
        {
            PoolId = d.PoolId,
            Name = d.Name,
            ScheduleStart = d.ScheduleStart,
            ScheduleEnd = d.ScheduleEnd,
            TablesVersion = d.TablesVersion
        };
    }

    public sealed class GachaPoolIndexVm
    {
        public GachaPoolFilterVm Filter { get; init; } = new();
        public IReadOnlyList<GachaPoolListItemVm> Items { get; init; } = Array.Empty<GachaPoolListItemVm>();
        public int Total { get; init; }
        public int Skip { get; init; }
        public int Take { get; init; } = 20;
    }

    public sealed class GachaPoolEntryRowVm : IValidatableObject
    {
        [Display(Name = "캐릭터")]
        [Required]
        public int CharacterId { get; set; }

        [Display(Name = "등급")]
        [Required]
        public short Grade { get; set; }

        [Display(Name = "픽업")]
        public bool RateUp { get; set; }

        [Display(Name = "가중치"), Range(1, int.MaxValue, ErrorMessage = "가중치는 1 이상이어야 합니다.")]
        public int Weight { get; set; } = 1;

        public IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            if (Weight <= 0)
                yield return new ValidationResult("가중치는 1 이상이어야 합니다.", new[] { nameof(Weight) });
        }
    }

    public sealed class GachaPoolFormVm : IValidatableObject
    {
        public int? PoolId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "풀 이름")]
        public string Name { get; set; } = default!;

        // 운영툴은 로컬 시간으로 입력 → 컨트롤러에서 UTC 변환
        [Required, DataType(DataType.DateTime)]
        [Display(Name = "시작(로컬)")]
        public DateTime? ScheduleStartLocal { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "종료(로컬)")]
        public DateTime? ScheduleEndLocal { get; set; }

        [Display(Name = "표 버전(스냅샷)")]
        public string? TablesVersion { get; set; }

        [Display(Name = "천장/보정(JSON)")]
        public string? PityJson { get; set; }

        [Display(Name = "기타 설정(JSON)")]
        public string? ConfigJson { get; set; }

        // 확률표
        [Display(Name = "엔트리(확률표)")]
        public List<GachaPoolEntryRowVm> Entries { get; set; } = new();

        // 드롭다운(캐릭터 선택용)
        public IEnumerable<SelectListItem>? CharacterOptions { get; set; }

        // ── 유효성
        public IEnumerable<ValidationResult> Validate(ValidationContext _)
        {
            if (ScheduleStartLocal is null)
                yield return new ValidationResult("시작 시간은 필수입니다.", new[] { nameof(ScheduleStartLocal) });

            if (ScheduleStartLocal is not null && ScheduleEndLocal is not null && ScheduleEndLocal <= ScheduleStartLocal)
                yield return new ValidationResult("종료 시간은 시작 시간보다 커야 합니다.", new[] { nameof(ScheduleEndLocal) });

            // 중복 캐릭터 체크
            var dup = Entries.GroupBy(x => x.CharacterId).FirstOrDefault(g => g.Count() > 1);
            if (dup is not null)
                yield return new ValidationResult($"캐릭터 #{dup.Key} 가 중복되었습니다.", new[] { nameof(Entries) });

            if (Entries.Any(e => e.Weight <= 0))
                yield return new ValidationResult("모든 엔트리의 가중치는 1 이상이어야 합니다.", new[] { nameof(Entries) });
        }

        // ── 매핑: VM → Requests (로컬시간 → UTC)
        public CreateGachaPoolRequest ToCreateRequest(string timeZoneId)
        {
            var (startUtc, endUtc) = ToUtc(timeZoneId);
            return new CreateGachaPoolRequest(
                Name: Name,
                ScheduleStart: startUtc,
                ScheduleEnd: endUtc,
                PityJson: PityJson,
                TablesVersion: TablesVersion,
                ConfigJson: ConfigJson
            );
        }

        public UpdateGachaPoolRequest ToUpdateRequest(string timeZoneId)
        {
            if (PoolId is null)
                throw new InvalidOperationException("PoolId is required for update.");

            var (startUtc, endUtc) = ToUtc(timeZoneId);
            return new UpdateGachaPoolRequest(
                PoolId: PoolId.Value,
                Name: Name,
                ScheduleStart: startUtc ?? DateTimeOffset.UtcNow,
                ScheduleEnd: endUtc,
                PityJson: PityJson,
                TablesVersion: TablesVersion,
                ConfigJson: ConfigJson
            );
        }

        public UpsertGachaPoolEntriesRequest ToUpsertEntriesRequest()
        {
            var lines = Entries.Select(e => new GachaPoolEntryDto(
                CharacterId: e.CharacterId,
                Grade: e.Grade,
                RateUp: e.RateUp,
                Weight: e.Weight
            )).ToList();

            return new UpsertGachaPoolEntriesRequest(PoolId ?? 0, lines);
        }

        // ── 매핑: DTO → VM (UTC → 로컬)
        public static GachaPoolFormVm FromDetailDto(GachaPoolDetailDto d, string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId)) timeZoneId = TimeZoneInfo.Local.Id;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var startLocal = TimeZoneInfo.ConvertTime(d.Pool.ScheduleStart.UtcDateTime, tz);
            DateTime? endLocal = d.Pool.ScheduleEnd.HasValue
                ? TimeZoneInfo.ConvertTime(d.Pool.ScheduleEnd.Value.UtcDateTime, tz)
                : null;

            return new GachaPoolFormVm
            {
                PoolId = d.Pool.PoolId,
                Name = d.Pool.Name,
                ScheduleStartLocal = startLocal,
                ScheduleEndLocal = endLocal,
                TablesVersion = d.Pool.TablesVersion,
                PityJson = d.PityJson,
                ConfigJson = d.ConfigJson,
                Entries = d.Entries.Select(x => new GachaPoolEntryRowVm
                {
                    CharacterId = x.CharacterId,
                    Grade = x.Grade,
                    RateUp = x.RateUp,
                    Weight = x.Weight
                }).ToList()
            };
        }

        private (DateTimeOffset? startUtc, DateTimeOffset? endUtc) ToUtc(string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId)) timeZoneId = TimeZoneInfo.Local.Id;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            DateTimeOffset? startUtc = null, endUtc = null;
            if (ScheduleStartLocal is DateTime s)
                startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(s, DateTimeKind.Unspecified), tz);
            if (ScheduleEndLocal is DateTime e)
                endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(e, DateTimeKind.Unspecified), tz);

            return (startUtc, endUtc);
        }
    }
}
