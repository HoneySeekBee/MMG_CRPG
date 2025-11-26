using Application.Users;
using Contracts.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServer.Monitoring;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/auth")]
    [Produces("application/x-protobuf")]
    [Consumes("application/x-protobuf")]
    public sealed class AuthProtoController : ControllerBase
    {
        private readonly IUserService _users;
        public AuthProtoController(IUserService users) => _users = users;

        private static string? GetStringProp(object obj, params string[] names)
        {
            var t = obj.GetType();
            foreach (var n in names)
            {
                var p = t.GetProperty(n);
                if (p != null)
                {
                    var v = p.GetValue(obj);
                    if (v != null) return v.ToString();
                }
            }
            return null;
        }
        private static object? GetObjProp(object obj, params string[] names)
        {
            var t = obj.GetType();
            foreach (var n in names)
            {
                var p = t.GetProperty(n);
                if (p != null) return p.GetValue(obj);
            }
            return null;
        }
        private static (string? access, string? refresh, string? playerId) ExtractTokensAndPlayerId(object dto)
        {
            // 최상단에서 먼저 시도
            var access = GetStringProp(dto, "AccessToken", "Access", "Token", "Jwt");
            var refresh = GetStringProp(dto, "RefreshToken", "Refresh");
            var player = GetStringProp(dto, "PlayerId", "UserId", "Id");

            // 못 찾았으면 하위 객체들 후보에서 탐색
            if (access == null || refresh == null)
            {
                var nested = GetObjProp(dto, "Tokens", "Auth", "Session", "Credentials");
                if (nested != null)
                {
                    access ??= GetStringProp(nested, "AccessToken", "Access", "Token", "Jwt");
                    refresh ??= GetStringProp(nested, "RefreshToken", "Refresh");
                }
            }
            if (player == null)
            {
                var nestedUser = GetObjProp(dto, "User", "Player", "Account");
                if (nestedUser != null)
                {
                    player = GetStringProp(nestedUser, "PlayerId", "UserId", "Id");
                }
            }

            return (access, refresh, player);
        }
        [AllowAnonymous]
        [HttpPost("register")]
        [Produces("application/x-protobuf")]
        [Consumes("application/x-protobuf")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterAuthRequest req, CancellationToken ct)
        {
            try
            {
                await _users.RegisterAsync(new RegisterUserRequest(req.Account, req.Password, req.Nickname), ct);

                var login = await _users.LoginAsync(new LoginUserRequest(req.Account, req.Password), ct);
                return Ok(new AuthResponse
                {
                    PlayerId = login.User.Id.ToString(),
                    AccessToken = login.Tokens.AccessToken,
                    RefreshToken = login.Tokens.RefreshToken ?? string.Empty,
                    ServerUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
            }
            catch (ArgumentException ex) when (ex.Message is "INVALID_ACCOUNT" or "INVALID_PASSWORD" or "INVALID_NICKNAME")
            {
                // 400: 형식 오류
                return BadRequest();
            }
            catch (InvalidOperationException ex) when (ex.Message == "ACCOUNT_TAKEN")
            {
                // 409: 중복 계정
                return Conflict();
            }
        }
        [AllowAnonymous]
        [HttpPost("guest")]
        public async Task<ActionResult<AuthResponse>> Guest([FromBody] GuestAuthRequest req, CancellationToken ct)
        {
            var login = await _users.LoginAsync(new LoginUserRequest("guestguest01", "guestguest01"), ct);

            return Ok(new AuthResponse
            {
                PlayerId = login.User.Id.ToString(),
                AccessToken = login.Tokens.AccessToken,
                RefreshToken = login.Tokens.RefreshToken ?? string.Empty,
                ServerUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [Consumes("application/x-protobuf")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginAuthRequest req, CancellationToken ct)
        {
            try
            {
                var login = await _users.LoginAsync(new LoginUserRequest(req.Account, req.Password), ct);

                if (login?.Tokens?.AccessToken is null)
                    return Problem(statusCode: 500, title: "LOGIN_TOKENS_MISSING");

                ServerMetrics.IncrementOnlineUsers();
                return Ok(new AuthResponse
                {
                    PlayerId = login.User.Id.ToString(),
                    AccessToken = login.Tokens.AccessToken,
                    RefreshToken = login.Tokens.RefreshToken ?? string.Empty,
                    ServerUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
            }
            catch (InvalidOperationException ex) when (ex.Message == "BAD_CREDENTIALS")
            {
                return Unauthorized(); // 401
            }
            catch (InvalidOperationException ex) when (ex.Message == "USER_SUSPENDED")
            {
                return Forbid();        // 403
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
        {
            try
            {
                // 레코드 생성자 사용 (CS7036 해결)
                var appReq = new RefreshTokenRequest(req.RefreshToken);

                var tokensDto = await _users.RefreshAsync(appReq, ct);
                var (access, refresh, playerId) = ExtractTokensAndPlayerId(tokensDto);

                if (access is null)
                    return Unauthorized();

                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return Ok(new AuthResponse
                {
                    PlayerId = playerId ?? string.Empty,
                    AccessToken = access,
                    RefreshToken = refresh ?? string.Empty,
                    ServerUnixMs = now
                });
            }
            catch (ArgumentException ex) when (ex.Message == "INVALID_REFRESH")
            {
                return BadRequest();
            }
            catch (InvalidOperationException ex) when (ex.Message is "INVALID_REFRESH" or "EXPIRED_REFRESH")
            {
                return Unauthorized();
            }
        }
    }
}
