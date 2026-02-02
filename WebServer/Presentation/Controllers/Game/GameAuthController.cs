using Application.Users;
using Contracts.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServer.Monitoring;

namespace Presentation.Controllers.Game
{
    [ApiController]
    [Route("api/pb/auth")]
    [Produces("application/x-protobuf")]
    [Consumes("application/x-protobuf")]
    public sealed class GameAuthController : ControllerBase
    {
        private readonly IUserService _users;
        public GameAuthController(IUserService users) => _users = users;

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
                var appReq = new RefreshTokenRequest(req.RefreshToken);
                var dto = await _users.RefreshAsync(appReq, ct);

                return Ok(new AuthResponse
                {
                    PlayerId = dto.UserId.ToString(),
                    AccessToken = dto.AccessToken,
                    RefreshToken = dto.RefreshToken,
                    ServerUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
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
