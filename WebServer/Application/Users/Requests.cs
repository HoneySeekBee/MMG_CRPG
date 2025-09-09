using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Users
{
    // ===== 조회용 쿼리 =====

    // 유저 목록 조회(운영툴)
    public sealed record UserListQuery(
        int Page = 1,
        int PageSize = 20,
        UserStatus? Status = null,     // Active/Suspended/Deleted 필터
        string? Search = null,         // account 또는 nickname like 검색
        DateTimeOffset? CreatedFrom = null,
        DateTimeOffset? CreatedTo = null
    );

    // 세션 목록 조회(운영툴)
    public sealed record SessionListQuery(
        int Page = 1,
        int PageSize = 20,
        int? UserId = null,
        bool? Revoked = null,
        bool ActiveOnly = false        // true면 만료 전 + 미취소만
    );

    // ===== 인증/계정 =====

    // 회원가입
    public sealed record RegisterUserRequest(
        string Account,
        string Password,
        string NickName
    );

    // 로그인
    public sealed record LoginUserRequest(
        string Account,
        string Password
    );

    // 토큰 갱신
    public sealed record RefreshTokenRequest(string RefreshToken);

    // 로그아웃(해당 리프레시 세션 무효화)
    public sealed record LogoutRequest(string RefreshToken);

    // 비밀번호 변경(사용자 본인)
    public sealed record ChangePasswordRequest(
        string OldPassword,
        string NewPassword
    );

    // ===== 프로필 =====

    // 프로필 수정(닉네임/아이콘 등)
    public sealed record UpdateProfileRequest(
        string NickName,
        int? IconId
    );

    // ===== 운영툴(관리자) =====

    // 상태 변경 (정지/해제/삭제 마크)
    public sealed record AdminSetStatusRequest(UserStatus Status);

    // 운영자가 닉네임 강제 수정
    public sealed record AdminSetNicknameRequest(string NickName);

    // 운영자 비번 초기화(강제 재설정)
    public sealed record AdminResetPasswordRequest(string NewPassword);

    // 특정 세션 취소 또는 전체 세션 강제 로그아웃
    public sealed record AdminRevokeSessionRequest(
        int? SessionId,          // null이면 로그인 세션 전체 대상
        bool AllOfUser = false   // true면 User의 모든 세션 취소
    );
}
