using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Users
{
    public sealed record UserSummaryDto(
        int Id,
        string Account,
        string NickName,
        short Level,
        int Gold,
        int Cristal,
        UserStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? LastLoginAt
    );

    // 프로필 단독 조회용(설정 화면 등)
    public sealed record UserProfileDto(
        int Id,          // ProfileId
        int UserId,
        string NickName,
        short Level,
        int Exp,
        int Gold,
        int Cristal,
        int? IconId
    );

    // 상세: 계정 + 프로필 + (선택) 최근 세션 요약
    public sealed record SessionBriefDto(
        int Id,
        DateTimeOffset ExpiresAt,
        DateTimeOffset RefreshExpiresAt,
        bool Revoked
    );

    public sealed record UserDetailDto(
        int Id,
        string Account,
        UserStatus Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? LastLoginAt,
        string NickName,
        short Level,
        int Exp,
        int Gold,
        int Cristal,
        int? IconId,
        IReadOnlyList<SessionBriefDto>? RecentSessions // null 가능
    );

    // 인증 토큰 DTO (로그인/리프레시 응답)
    public sealed record AuthTokensDto(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset AccessExpiresAt,
        DateTimeOffset RefreshExpiresAt
    );

    // 로그인 결과: 요약 + 토큰
    public sealed record LoginResultDto(
        UserSummaryDto User,
        AuthTokensDto Tokens
    );

    // 공통 페이지 결과 (캐릭터와 동일 패턴)
    public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
    public sealed record SecurityEventBriefDto(
        int Id,
        int UserId,
        string Type,          // ex) LoginSuccess, LoginFail, TokenRefresh, Logout
        string? MetaJson,     // jsonb 원문 (nullable)
        DateTimeOffset CreatedAt
    );
    // ===== Mapping =====

}
