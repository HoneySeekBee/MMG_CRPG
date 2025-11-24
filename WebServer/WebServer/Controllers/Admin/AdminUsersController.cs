using Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    public sealed class AdminUsersController : ControllerBase
    {
        private readonly IUserService _users;
        public AdminUsersController(IUserService users) => _users = users;

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserSummaryDto>>> GetUsers([FromQuery] UserListQuery query, CancellationToken ct)
        {
            var res = await _users.GetListAsync(query, ct);
            return Ok(res);
        }

        [HttpGet("{userId:int}/sessions")]
        public async Task<ActionResult<PagedResult<SessionBriefDto>>> GetSessions([FromRoute] int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? revoked = null, [FromQuery] bool activeOnly = false, CancellationToken ct = default)
        {
            var q = new SessionListQuery(Page: page, PageSize: pageSize, UserId: userId, Revoked: revoked, ActiveOnly: activeOnly);
            var res = await _users.GetSessionsAsync(q, ct);
            return Ok(res);
        }

        [HttpPost("{userId:int}/status")]
        public async Task<IActionResult> SetStatus([FromRoute] int userId, [FromBody] AdminSetStatusRequest req, CancellationToken ct)
        {
            try
            {
                await _users.AdminSetStatusAsync(userId, req, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message == "USER_NOT_FOUND")
            {
                return NotFound(new { code = ex.Message });
            }
        }

        [HttpPost("{userId:int}/nickname")]
        public async Task<IActionResult> SetNickname([FromRoute] int userId, [FromBody] AdminSetNicknameRequest req, CancellationToken ct)
        {
            try
            {
                await _users.AdminSetNicknameAsync(userId, req, ct);
                return NoContent();
            }
            catch (ArgumentException ex) when (ex.Message == "INVALID_NICKNAME")
            {
                return BadRequest(new { code = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "PROFILE_NOT_FOUND")
            {
                return NotFound(new { code = ex.Message });
            }
        }

        [HttpPost("{userId:int}/reset-password")]
        public async Task<IActionResult> ResetPassword([FromRoute] int userId, [FromBody] AdminResetPasswordRequest req, CancellationToken ct)
        {
            try
            {
                await _users.AdminResetPasswordAsync(userId, req, ct);
                return NoContent();
            }
            catch (ArgumentException ex) when (ex.Message == "INVALID_PASSWORD")
            {
                return BadRequest(new { code = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message == "USER_NOT_FOUND")
            {
                return NotFound(new { code = ex.Message });
            }
        }

        [HttpPost("{userId:int}/revoke-session")]
        public async Task<IActionResult> RevokeSession([FromRoute] int userId, [FromBody] AdminRevokeSessionRequest req, CancellationToken ct)
        {
            try
            {
                await _users.AdminRevokeSessionAsync(userId, req, ct);
                return NoContent();
            }
            catch (ArgumentException ex) when (ex.Message == "SESSION_ID_REQUIRED")
            {
                return BadRequest(new { code = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message is "SESSION_NOT_FOUND" or "SESSION_USER_MISMATCH")
            {
                return NotFound(new { code = ex.Message });
            }
        }
    }
}
