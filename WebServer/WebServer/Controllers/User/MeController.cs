using Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebServer.Controllers.User
{
    [ApiController]
    [Route("api/me")]
    [Authorize]
    public sealed class MeController : ControllerBase
    {
        private readonly IUserService _users;
        public MeController(IUserService users) => _users = users;

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new InvalidOperationException("NO_USER_ID"));

        [HttpGet("summary")]
        public async Task<ActionResult<UserSummaryDto>> GetSummary(CancellationToken ct)
        {
            var id = GetUserId();
            try
            {
                var dto = await _users.GetMySummaryAsync(id, ct);
                return Ok(dto);
            }
            catch (InvalidOperationException ex) when (ex.Message is "USER_NOT_FOUND" or "PROFILE_NOT_FOUND")
            {
                return NotFound(new { code = ex.Message });
            }
        }

        [HttpGet("detail")]
        public async Task<ActionResult<UserDetailDto>> GetDetail(CancellationToken ct)
        {
            var id = GetUserId();
            try
            {
                var dto = await _users.GetDetailAsync(id, ct);
                return Ok(dto);
            }
            catch (InvalidOperationException ex) when (ex.Message == "USER_NOT_FOUND")
            {
                return NotFound(new { code = ex.Message });
            }
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile(CancellationToken ct)
        {
            var id = GetUserId();
            try
            {
                var dto = await _users.GetProfileAsync(id, ct);
                return Ok(dto);
            }
            catch (InvalidOperationException ex) when (ex.Message == "PROFILE_NOT_FOUND")
            {
                return NotFound(new { code = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest req, CancellationToken ct)
        {
            var id = GetUserId();
            try
            {
                var dto = await _users.UpdateProfileAsync(id, req, ct);
                return Ok(dto);
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

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
        {
            var id = GetUserId();
            try
            {
                await _users.ChangePasswordAsync(id, req, ct);
                return NoContent();
            }
            catch (ArgumentException ex) when (ex.Message == "INVALID_PASSWORD")
            {
                return BadRequest(new { code = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message is "USER_NOT_FOUND" or "BAD_CREDENTIALS")
            {
                return Unauthorized(new { code = ex.Message });
            }
        }
    }
}
