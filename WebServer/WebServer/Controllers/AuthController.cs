using Application.UserCurrency;
using Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IUserService _users;
        public AuthController(IUserService users) => _users = users;

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<int>> Register([FromBody] RegisterUserRequest req, CancellationToken ct)
        {
            try
            {
                var id = await _users.RegisterAsync(req, ct);
                return CreatedAtAction(nameof(Register), new { id }, id);
            }
            catch (ArgumentException ex) when (ex.Message is "INVALID_ACCOUNT" or "INVALID_PASSWORD" or "INVALID_NICKNAME")
            {
                return BadRequest(new { code = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "ACCOUNT_TAKEN")
            {
                return Conflict(new { code = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginUserRequest req, CancellationToken ct)
        {
            try
            {
                var res = await _users.LoginAsync(req, ct);
                return Ok(res);
            }
            catch (InvalidOperationException ex) when (ex.Message is "BAD_CREDENTIALS")
            {
                return Unauthorized(new { code = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message is "USER_SUSPENDED")
            {
                return Forbid();
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthTokensDto>> Refresh([FromBody] RefreshTokenRequest req, CancellationToken ct)
        {
            try
            {
                var res = await _users.RefreshAsync(req, ct);
                return Ok(res);
            }
            catch (ArgumentException ex) when (ex.Message == "INVALID_REFRESH")
            {
                return BadRequest(new { code = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message is "INVALID_REFRESH" or "EXPIRED_REFRESH")
            {
                return Unauthorized(new { code = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest req, CancellationToken ct)
        {
            await _users.LogoutAsync(req, ct);
            return NoContent();
        }
    }
}
