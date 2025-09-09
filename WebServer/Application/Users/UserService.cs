using Application.Repositories;
using Domain.Entities;
using Domain.Enum;

namespace Application.Users
{
    public sealed class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IUserQueryRepository _userQuery;
        private readonly IProfileRepository _profiles;
        private readonly ISessionRepository _sessions;
        private readonly ISessionQueryRepository _sessionQuery;
        private readonly ISecurityEventRepository _sec;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokens;
        private readonly IClock _clock;

        public UserService(
            IUserRepository users,
            IUserQueryRepository userQuery,
            IProfileRepository profiles,
            ISessionRepository sessions,
            ISessionQueryRepository sessionQuery,
            ISecurityEventRepository sec,
            IPasswordHasher hasher,
            ITokenService tokens,
            IClock clock)
        {
            _users = users;
            _userQuery = userQuery;
            _profiles = profiles;
            _sessions = sessions;
            _sessionQuery = sessionQuery;
            _sec = sec;
            _hasher = hasher;
            _tokens = tokens;
            _clock = clock;
        }

        // --- 인증/계정 -------------------------------------------------------

        public async Task<int> RegisterAsync(RegisterUserRequest req, CancellationToken ct)
        {
            // 1) 검증
            var account = (req.Account ?? string.Empty).Trim();
            if (account.Length is < 4 or > 64) throw new ArgumentException("INVALID_ACCOUNT");
            if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8) throw new ArgumentException("INVALID_PASSWORD");
            if (string.IsNullOrWhiteSpace(req.NickName)) throw new ArgumentException("INVALID_NICKNAME");

            // 2) 중복
            if (await _users.ExistsByAccountAsync(account, ct)) throw new InvalidOperationException("ACCOUNT_TAKEN");

            // 3) 생성
            var hash = _hasher.Hash(req.Password);
            var user = User.Create(account, hash, _clock.UtcNow);
            await _users.AddAsync(user, ct);
            await _users.SaveChangesAsync(ct); // Id 발급

            var profile = UserProfile.Create(user.Id, req.NickName.Trim());
            await _profiles.AddAsync(profile, ct);
            await _profiles.SaveChangesAsync(ct);

            // 4) 이벤트
            await _sec.AddAsync(SecurityEvent.Create(SecurityEventType.LoginSuccess, _clock.UtcNow, user.Id, "{\"register\":true}"), ct);
            await _sec.SaveChangesAsync(ct);

            return user.Id;
        }

        public async Task<LoginResultDto> LoginAsync(LoginUserRequest req, CancellationToken ct)
        {
            var account = (req.Account ?? string.Empty).Trim();

            var user = await _users.FindByAccountAsync(account, ct)
                       ?? throw new InvalidOperationException("BAD_CREDENTIALS");
            if (user.Status != UserStatus.Active) throw new InvalidOperationException("USER_SUSPENDED");

            if (!_hasher.Verify(req.Password, user.PasswordHash))
            {
                await _sec.AddAsync(SecurityEvent.Create(SecurityEventType.LoginFail, _clock.UtcNow, user.Id), ct);
                await _sec.SaveChangesAsync(ct);
                throw new InvalidOperationException("BAD_CREDENTIALS");
            }

            var profile = await _profiles.GetByUserIdAsync(user.Id, ct)
                          ?? throw new InvalidOperationException("PROFILE_NOT_FOUND");

            // 토큰 생성 + 세션 저장(해시만)
            var (access, accessExp) = _tokens.CreateAccessToken(user);
            var (refresh, refreshExp) = _tokens.CreateRefreshToken(user);

            var session = Session.Create(
                userId: user.Id,
                accessTokenHash: _tokens.Hash(access),
                refreshTokenHash: _tokens.Hash(refresh),
                expiresAt: accessExp,
                refreshExpiresAt: refreshExp,
                now: _clock.UtcNow);

            await _sessions.AddAsync(session, ct);
            await _sessions.SaveChangesAsync(ct);

            user.TouchLogin(_clock.UtcNow);
            await _users.SaveChangesAsync(ct);

            await _sec.AddAsync(SecurityEvent.Create(SecurityEventType.LoginSuccess, _clock.UtcNow, user.Id), ct);
            await _sec.SaveChangesAsync(ct);

            var userDto = user.ToSummaryDto(profile);
            var tokenDto = new AuthTokensDto(access, refresh, accessExp, refreshExp);
            return new LoginResultDto(userDto, tokenDto);
        }

        public async Task<AuthTokensDto> RefreshAsync(RefreshTokenRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken)) throw new ArgumentException("INVALID_REFRESH");

            var hash = _tokens.Hash(req.RefreshToken);
            var session = await _sessions.FindByRefreshHashAsync(hash, ct)
                          ?? throw new InvalidOperationException("INVALID_REFRESH");

            if (session.Revoked || session.IsRefreshExpired(_clock.UtcNow))
                throw new InvalidOperationException("EXPIRED_REFRESH");

            var user = await _users.GetByIdAsync(session.UserId, ct)
                       ?? throw new InvalidOperationException("USER_NOT_FOUND");
            if (user.Status != UserStatus.Active) throw new InvalidOperationException("USER_SUSPENDED");

            var (access, accessExp) = _tokens.CreateAccessToken(user);

            // 보안상 refresh도 롤링(선호에 따라 off 가능)
            var (newRefresh, newRefreshExp) = _tokens.CreateRefreshToken(user);

            session.RotateAccess(_tokens.Hash(access), accessExp);
            session.RotateRefresh(_tokens.Hash(newRefresh), newRefreshExp);
            await _sessions.SaveChangesAsync(ct);

            await _sec.AddAsync(SecurityEvent.Create(SecurityEventType.TokenRefresh, _clock.UtcNow, user.Id), ct);
            await _sec.SaveChangesAsync(ct);

            return new AuthTokensDto(access, newRefresh, accessExp, newRefreshExp);
        }

        public async Task LogoutAsync(LogoutRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken)) return;

            var hash = _tokens.Hash(req.RefreshToken);
            var session = await _sessions.FindByRefreshHashAsync(hash, ct);
            if (session is null) return;

            session.Revoke();
            await _sessions.SaveChangesAsync(ct);

            await _sec.AddAsync(SecurityEvent.Create(SecurityEventType.Logout, _clock.UtcNow, session.UserId), ct);
            await _sec.SaveChangesAsync(ct);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.OldPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
                throw new ArgumentException("INVALID_PASSWORD");
            if (req.NewPassword.Length < 8) throw new ArgumentException("INVALID_PASSWORD");

            var u = await _users.GetByIdAsync(userId, ct) ?? throw new InvalidOperationException("USER_NOT_FOUND");

            if (!_hasher.Verify(req.OldPassword, u.PasswordHash))
                throw new InvalidOperationException("BAD_CREDENTIALS");

            var newHash = _hasher.Hash(req.NewPassword);
            u.SetPasswordHash(newHash);
            await _users.SaveChangesAsync(ct);

            // 필요 시: 모든 세션 무효화(선호에 따라)
            // await _sessions.InvalidateAllByUserIdAsync(userId, ct); await _sessions.SaveChangesAsync(ct);
        }

        // --- 내 정보 / 프로필 -------------------------------------------------

        public async Task<UserSummaryDto> GetMySummaryAsync(int userId, CancellationToken ct)
        {
            var u = await _users.GetByIdAsync(userId, ct) ?? throw new InvalidOperationException("USER_NOT_FOUND");
            var p = await _profiles.GetByUserIdAsync(userId, ct) ?? throw new InvalidOperationException("PROFILE_NOT_FOUND");
            return u.ToSummaryDto(p);
        }

        public async Task<UserDetailDto> GetDetailAsync(int userId, CancellationToken ct)
        {
            var (u, p) = await _userQuery.GetAggregateAsync(userId, ct);
            if (u is null || p is null) throw new InvalidOperationException("USER_NOT_FOUND");

            var recent = await _sessionQuery.GetRecentByUserIdAsync(userId, 5, ct);
            return u.ToDetailDto(p, recent);
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken ct)
        {
            var p = await _profiles.GetByUserIdAsync(userId, ct) ?? throw new InvalidOperationException("PROFILE_NOT_FOUND");
            return p.ToProfileDto();
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.NickName)) throw new ArgumentException("INVALID_NICKNAME");

            var p = await _profiles.GetByUserIdAsync(userId, ct) ?? throw new InvalidOperationException("PROFILE_NOT_FOUND");
            p.Rename(req.NickName.Trim());
            p.SetIcon(req.IconId);
            await _profiles.SaveChangesAsync(ct);
            return p.ToProfileDto();
        }

        // --- 목록 / 세션(운영툴) ---------------------------------------------

        public async Task<PagedResult<UserSummaryDto>> GetListAsync(UserListQuery query, CancellationToken ct)
        {
            if (query.Page <= 0) throw new ArgumentOutOfRangeException(nameof(query.Page));
            if (query.PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(query.PageSize));

            var (rows, total) = await _userQuery.GetPagedAsync(query, ct);

            var items = rows.Select(x => x.User.ToSummaryDto(x.Profile)).ToList();
            return new PagedResult<UserSummaryDto>(items, total, query.Page, query.PageSize);
        }

        public async Task<PagedResult<SessionBriefDto>> GetSessionsAsync(SessionListQuery query, CancellationToken ct)
        {
            if (query.Page <= 0) throw new ArgumentOutOfRangeException(nameof(query.Page));
            if (query.PageSize <= 0) throw new ArgumentOutOfRangeException(nameof(query.PageSize));

            var (rows, total) = await _sessionQuery.GetPagedAsync(query, ct);
            var items = rows.Select(s => s.ToBriefDto()).ToList();
            return new PagedResult<SessionBriefDto>(items, total, query.Page, query.PageSize);
        }

        // --- 운영자 액션 ------------------------------------------------------

        public async Task AdminSetStatusAsync(int userId, AdminSetStatusRequest req, CancellationToken ct)
        {
            var u = await _users.GetByIdAsync(userId, ct) ?? throw new InvalidOperationException("USER_NOT_FOUND");

            switch (req.Status)
            {
                case UserStatus.Active: u.Activate(); break;
                case UserStatus.Suspended: u.Suspend(); break;
                case UserStatus.Deleted: u.MarkDeleted(); break;
                default: throw new ArgumentOutOfRangeException(nameof(req.Status));
            }

            await _users.SaveChangesAsync(ct);
        }

        public async Task AdminSetNicknameAsync(int userId, AdminSetNicknameRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.NickName)) throw new ArgumentException("INVALID_NICKNAME");

            var p = await _profiles.GetByUserIdAsync(userId, ct) ?? throw new InvalidOperationException("PROFILE_NOT_FOUND");
            p.Rename(req.NickName.Trim());
            await _profiles.SaveChangesAsync(ct);
        }

        public async Task AdminResetPasswordAsync(int userId, AdminResetPasswordRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 8)
                throw new ArgumentException("INVALID_PASSWORD");

            var u = await _users.GetByIdAsync(userId, ct) ?? throw new InvalidOperationException("USER_NOT_FOUND");

            var newHash = _hasher.Hash(req.NewPassword);
            u.SetPasswordHash(newHash);
            await _users.SaveChangesAsync(ct);

            // 보안 정책에 따라 모든 세션 만료 권장
            await _sessions.InvalidateAllByUserIdAsync(userId, ct);
            await _sessions.SaveChangesAsync(ct);
        }

        public async Task AdminRevokeSessionAsync(int userId, AdminRevokeSessionRequest req, CancellationToken ct)
        {
            if (req.AllOfUser)
            {
                await _sessions.InvalidateAllByUserIdAsync(userId, ct);
                await _sessions.SaveChangesAsync(ct);
                return;
            }

            if (req.SessionId is not int sid) throw new ArgumentException("SESSION_ID_REQUIRED");

            var session = await _sessions.FindByIdAsync(sid, ct) ?? throw new InvalidOperationException("SESSION_NOT_FOUND");
            if (session.UserId != userId) throw new InvalidOperationException("SESSION_USER_MISMATCH");

            session.Revoke();
            await _sessions.SaveChangesAsync(ct);
        }
    }
}

