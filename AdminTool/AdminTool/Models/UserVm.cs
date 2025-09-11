using Application.Users;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AdminTool.Models
{
    public sealed class PaginationVm
    {
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
    public sealed class AdminLoginVm
    {
        [Required, Display(Name = "계정")]
        public string Account { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Display(Name = "비밀번호")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
    public sealed class AdminRegisterVm
    {
        [Required, StringLength(64, MinimumLength = 4), Display(Name = "계정")]
        public string Account { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password), Display(Name = "비밀번호")]
        public string Password { get; set; } = string.Empty;

        [Required, StringLength(100), Display(Name = "닉네임")]
        public string NickName { get; set; } = string.Empty;
    }
    public sealed class UserSearchVm
    {
        [Display(Name = "검색어 (계정/닉네임)")]
        public string? Query { get; set; }

        [Display(Name = "상태")]
        public UserStatus? Status { get; set; }

        [Display(Name = "가입일 From")]
        public DateTimeOffset? CreatedFrom { get; set; }

        [Display(Name = "가입일 To")]
        public DateTimeOffset? CreatedTo { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 200)]
        public int PageSize { get; set; } = 20;

        public IEnumerable<SelectListItem> StatusOptions =>
            new[]
            {
                new SelectListItem("전체", "", selected: Status is null),
                new SelectListItem("Active",    UserStatus.Active.ToString(),    Status == UserStatus.Active),
                new SelectListItem("Suspended", UserStatus.Suspended.ToString(), Status == UserStatus.Suspended),
                new SelectListItem("Deleted",   UserStatus.Deleted.ToString(),   Status == UserStatus.Deleted),
            };
    }

    public sealed class UserListItemVm
    {
        public int Id { get; init; }
        public string Account { get; init; } = string.Empty;
        public string NickName { get; init; } = string.Empty;
        public short Level { get; init; }
        public int Gold { get; init; }
        public int Gem { get; init; }
        public int Token { get; init; }
        public UserStatus Status { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? LastLoginAt { get; init; }
    }

    public sealed class UserListVm
    {
        public UserSearchVm Search { get; init; } = new();
        public IReadOnlyList<UserListItemVm> Items { get; init; } = Array.Empty<UserListItemVm>();
        public PaginationVm Paging { get; init; } = new();
    }

    // ===== Users: 상세 =====
    public sealed class SessionListItemVm
    {
        public int Id { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }
        public DateTimeOffset RefreshExpiresAt { get; init; }
        public bool Revoked { get; init; }
    }

    public sealed class UserDetailVm
    {
        // 요약
        public int Id { get; init; }
        public string Account { get; init; } = string.Empty;
        public UserStatus Status { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? LastLoginAt { get; init; }

        // 프로필
        public string NickName { get; init; } = string.Empty;
        public short Level { get; init; }
        public int Exp { get; init; }
        public int Gold { get; init; }
        public int Gem { get; init; }
        public int Token { get; init; }
        public int? IconId { get; init; }

        // 최근 세션
        public IReadOnlyList<SessionListItemVm> RecentSessions { get; init; } = Array.Empty<SessionListItemVm>();
    }

    // ===== Users: 액션 폼 =====
    public sealed class SetStatusVm
    {
        [Required]
        public int UserId { get; set; }
        [Required, Display(Name = "상태")]
        public UserStatus Status { get; set; }
    }

    public sealed class SetNicknameVm
    {
        [Required]
        public int UserId { get; set; }
        [Required, StringLength(100), Display(Name = "닉네임")]
        public string NickName { get; set; } = string.Empty;
    }

    public sealed class ResetPasswordVm
    {
        [Required]
        public int UserId { get; set; }
        [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password), Display(Name = "새 비밀번호")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public sealed class RevokeSessionVm
    {
        [Required]
        public int UserId { get; set; }
        [Display(Name = "세션 ID")]
        public int? SessionId { get; set; }

        [Display(Name = "해당 유저의 모든 세션 만료")]
        public bool AllOfUser { get; set; }
    }

    // ===== Security Events (간단 뷰어용) =====
    public sealed class SecurityEventSearchVm
    {
        public int? UserId { get; set; }
        public string? Type { get; set; } // LoginSuccess/LoginFail/TokenRefresh/Logout...
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
        [Range(1, 200)] public int PageSize { get; set; } = 20;
    }

    public sealed class SecurityEventItemVm
    {
        public int Id { get; init; }
        public int? UserId { get; init; }
        public string Type { get; init; } = string.Empty;
        public string? MetaJson { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }

    public sealed class SecurityEventListVm
    {
        public SecurityEventSearchVm Search { get; init; } = new();
        public IReadOnlyList<SecurityEventItemVm> Items { get; init; } = Array.Empty<SecurityEventItemVm>();
        public PaginationVm Paging { get; init; } = new();
    }

    // ===== Mapping helpers (DTO → VM) =====\
    public static class UserVmMappings
    {
        public static UserListVm ToVm(
            this Application.Common.Models.PagedResult<Application.Users.UserSummaryDto> page,
            UserSearchVm search) =>
            new()
            {
                Search = search,
                Items = MapList(page.Items),
                Paging = new PaginationVm
                {
                    Page = page.Page,
                    PageSize = page.PageSize,
                    TotalCount = (int)page.TotalCount
                }
            };

        public static IReadOnlyList<UserListItemVm> MapList(IReadOnlyList<UserSummaryDto> items) =>
            items.Select(x => new UserListItemVm
            {
                Id = x.Id,
                Account = x.Account,
                NickName = x.NickName,
                Level = x.Level,
                Gold = x.Gold,
                Gem = x.Gem,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                LastLoginAt = x.LastLoginAt
            }).ToList();

        public static UserDetailVm ToVm(this UserDetailDto d) =>
            new()
            {
                Id = d.Id,
                Account = d.Account,
                Status = d.Status,
                CreatedAt = d.CreatedAt,
                LastLoginAt = d.LastLoginAt,
                NickName = d.NickName,
                Level = d.Level,
                Exp = d.Exp,
                Gold = d.Gold,
                Gem = d.Gem,
                Token = d.Token,
                IconId = d.IconId,
                RecentSessions = d.RecentSessions is null
                    ? Array.Empty<SessionListItemVm>()
                    : MapSessions(d.RecentSessions)
            };

        public static IReadOnlyList<SessionListItemVm> MapSessions(IReadOnlyList<SessionBriefDto> list) =>
            list.Select(s => new SessionListItemVm
            {
                Id = s.Id,
                ExpiresAt = s.ExpiresAt,
                RefreshExpiresAt = s.RefreshExpiresAt,
                Revoked = s.Revoked
            }).ToList();
    }
}
