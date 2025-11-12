using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Users
{
    public interface IUserService
    {
        // ===== 인증/계정 =====

        /// <summary>회원가입 (중복 아이디 검사 포함)</summary>
        Task<int> RegisterAsync(RegisterUserRequest req, CancellationToken ct);

        /// <summary>로그인 → 유저 요약 + 액세스/리프레시 토큰</summary>
        Task<LoginResultDto> LoginAsync(LoginUserRequest req, CancellationToken ct);

        /// <summary>리프레시 토큰으로 액세스/리프레시 재발급</summary>
        Task<AuthTokensDto> RefreshAsync(RefreshTokenRequest req, CancellationToken ct);

        /// <summary>리프레시 토큰 기반 로그아웃(세션 무효화)</summary>
        Task LogoutAsync(LogoutRequest req, CancellationToken ct);

        /// <summary>비밀번호 변경(본인 인증 전제: 로그인 상태)</summary>
        Task ChangePasswordAsync(int userId, ChangePasswordRequest req, CancellationToken ct);


        // ===== 내 정보 / 프로필 =====

        /// <summary>내 요약 정보(계정+프로필 합본)</summary>
        Task<UserSummaryDto> GetMySummaryAsync(int userId, CancellationToken ct);

        /// <summary>유저 상세(계정+프로필 + 선택 세션 브리프)</summary>
        Task<UserDetailDto> GetDetailAsync(int userId, CancellationToken ct);

        /// <summary>프로필 단독 조회</summary>
        Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken ct);

        /// <summary>프로필 수정(닉네임/아이콘)</summary>
        Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest req, CancellationToken ct);


        // ===== 목록/세션(운영툴) =====

        /// <summary>유저 목록 조회(페이징/필터)</summary>
        Task<PagedResult<UserSummaryDto>> GetListAsync(UserListQuery query, CancellationToken ct);

        /// <summary>세션 목록 조회(페이징/필터)</summary>
        Task<PagedResult<SessionBriefDto>> GetSessionsAsync(SessionListQuery query, CancellationToken ct);


        // ===== 운영자 액션 =====

        /// <summary>유저 상태 변경(Active/Suspended/Deleted)</summary>
        Task AdminSetStatusAsync(int userId, AdminSetStatusRequest req, CancellationToken ct);

        /// <summary>운영자 닉네임 강제 변경</summary>
        Task AdminSetNicknameAsync(int userId, AdminSetNicknameRequest req, CancellationToken ct);

        /// <summary>운영자 비밀번호 초기화</summary>
        Task AdminResetPasswordAsync(int userId, AdminResetPasswordRequest req, CancellationToken ct);

        /// <summary>특정 세션 또는 유저의 모든 세션 무효화</summary>
        Task AdminRevokeSessionAsync(int userId, AdminRevokeSessionRequest req, CancellationToken ct);
    }
}
