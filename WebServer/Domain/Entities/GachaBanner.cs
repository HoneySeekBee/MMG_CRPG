using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class GachaBanner
    {
        // --- 생성/식별 ---
        private GachaBanner() { }

        public int Id { get; set; }                 // PK (DB: serial/bigserial)
        public string Key { get; private set; } = default!; // 운영툴/시드 참조용, UNIQUE

        // --- 표시/연결 ---
        public string Title { get; private set; } = default!;
        public string? Subtitle { get; private set; }
        public int? PortraitId { get; private set; }        // 배너 이미지 참조
        public int GachaPoolId { get; private set; }        // 어떤 풀을 띄우는지

        // --- 일정/상태/우선순위 ---
        public DateTimeOffset StartsAt { get; private set; }
        public DateTimeOffset? EndsAt { get; private set; }
        public short Priority { get; private set; }         // 클수록 우선
        public GachaBannerStatus Status { get; private set; } = GachaBannerStatus.Live;
        public bool IsActive { get; private set; } = true;

        // --- 계산/편의 ---
        public bool IsLiveNow(DateTimeOffset? now = null)
        {
            var t = now ?? DateTimeOffset.UtcNow;
            if (!IsActive || Status != GachaBannerStatus.Live) return false;
            if (t < StartsAt) return false;
            if (EndsAt is { } end && t >= end) return false;
            return true;
        }

        public bool IsScheduled => Status == GachaBannerStatus.Scheduled;
        public bool HasEnded(DateTimeOffset? now = null)
            => EndsAt is { } end && (now ?? DateTimeOffset.UtcNow) >= end;

        // --- 팩토리 ---
        public static GachaBanner Create(
            string key,
            string title,
            int gachaPoolId,
            DateTimeOffset? startsAt = null,
            DateTimeOffset? endsAt = null,
            string? subtitle = null,
            int? portraitId = null,
            short priority = 0,
            GachaBannerStatus status = GachaBannerStatus.Live,
            bool isActive = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key is required", nameof(key));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("title is required", nameof(title));
            if (gachaPoolId <= 0)
                throw new ArgumentOutOfRangeException(nameof(gachaPoolId));

            var start = startsAt ?? DateTimeOffset.UtcNow;
            if (endsAt is { } e && e <= start)
                throw new ArgumentException("EndsAt must be greater than StartsAt", nameof(endsAt));

            return new GachaBanner
            {
                Key = key.Trim(),
                Title = title.Trim(),
                Subtitle = string.IsNullOrWhiteSpace(subtitle) ? null : subtitle.Trim(),
                PortraitId = portraitId,
                GachaPoolId = gachaPoolId,
                StartsAt = start,
                EndsAt = endsAt,
                Priority = priority,
                Status = status,
                IsActive = isActive
            };
        }

        // --- 동작 메서드(명령) ---
        public void Rename(string title, string? subtitle = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("title is required", nameof(title));
            Title = title.Trim();
            Subtitle = string.IsNullOrWhiteSpace(subtitle) ? null : subtitle.Trim();
        }

        public void LinkPortrait(int? portraitId) => PortraitId = portraitId;

        public void LinkPool(int gachaPoolId)
        {
            if (gachaPoolId <= 0) throw new ArgumentOutOfRangeException(nameof(gachaPoolId));
            GachaPoolId = gachaPoolId;
        }

        public void Reschedule(DateTimeOffset startsAt, DateTimeOffset? endsAt)
        {
            if (endsAt is { } e && e <= startsAt)
                throw new ArgumentException("EndsAt must be greater than StartsAt", nameof(endsAt));
            StartsAt = startsAt;
            EndsAt = endsAt;
        }

        public void SetPriority(short priority) => Priority = priority;

        public void SetStatus(GachaBannerStatus status) => Status = status;

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        // 간단한 가드: 라이브 상태로 전환할 때 시간이 말이 되는지 체크
        public void GoLiveNow()
        {
            Status = GachaBannerStatus.Live;
            StartsAt = DateTimeOffset.UtcNow;
            if (EndsAt is { } e && e <= StartsAt)
                EndsAt = null; // 잘못 잡힌 종료는 제거
            IsActive = true;
        }
    }
}
