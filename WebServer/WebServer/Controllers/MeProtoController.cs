using Application.Users;
using Contracts.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/me")]
    [Authorize]
    [Produces("application/x-protobuf")]
    public sealed class MeProtoController : ControllerBase
    {
        private readonly IUserService _users;
        public MeProtoController(IUserService users) => _users = users;

        private int CurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("NO_USER_ID"));

        [HttpGet("summary")]
        public async Task<ActionResult<UserSummaryPb>> Summary(CancellationToken ct)
        {
            try
            {
                var s = await _users.GetMySummaryAsync(CurrentUserId(), ct);
                return Ok(new UserSummaryPb
                {
                    UserId = s.Id,
                    Nickname = s.NickName,
                    Level = s.Level,
                    Gold = s.Gold,
                    Gem = s.Gem,
                    Token = s.Token,
                    IconId = s.IconId ?? 0,
                });
            }
            catch (InvalidOperationException ex) when (ex.Message is "USER_NOT_FOUND" or "PROFILE_NOT_FOUND")
            {
                return NotFound();
            }
        }

        // GET /api/pb/me/profile
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfilePb>> Profile(CancellationToken ct)
        {
            try
            {
                var p = await _users.GetProfileAsync(CurrentUserId(), ct);
                return Ok(new UserProfilePb
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Nickname = p.NickName,
                    Level = p.Level,
                    Exp = p.Exp,
                    Gold = p.Gold,
                    Gem = p.Gem,
                    Token = p.Token,
                    IconId = p.IconId ?? 0,
                });
            }
            catch (InvalidOperationException ex) when (ex.Message == "PROFILE_NOT_FOUND")
            {
                return NotFound();
            }
        }

        // PUT /api/pb/me/profile
        [HttpPut("profile")]
        [Consumes("application/x-protobuf")]
        public async Task<ActionResult<UserProfilePb>> Update([FromBody] UpdateProfilePb req, CancellationToken ct)
        {
            try
            {
                var u = await _users.UpdateProfileAsync(
                    CurrentUserId(),
                    new UpdateProfileRequest(req.Nickname, req.IconId == 0 ? (int?)null : req.IconId),
                    ct);

                return Ok(new UserProfilePb
                {
                    Id = u.Id,
                    UserId = u.UserId,
                    Nickname = u.NickName,
                    Level = u.Level,
                    Exp = u.Exp,
                    Gold = u.Gold,
                    Gem = u.Gem,
                    Token = u.Token,
                    IconId = u.IconId ?? 0,
                });
            }
            catch (ArgumentException ex) when (ex.Message == "INVALID_NICKNAME")
            {
                return BadRequest(); // 400
            }
            catch (InvalidOperationException ex) when (ex.Message == "PROFILE_NOT_FOUND")
            {
                return NotFound();   // 404
            }
        }

        // POST /api/pb/me/change-password
        [HttpPost("change-password")]
        [Consumes("application/x-protobuf")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordPb req, CancellationToken ct)
        {
            try
            {
                await _users.ChangePasswordAsync(
                    CurrentUserId(),
                    new ChangePasswordRequest(req.OldPassword, req.NewPassword),
                    ct);
                return NoContent();
            }
            catch (ArgumentException ex) when (ex.Message == "INVALID_PASSWORD")
            {
                return BadRequest();    // 400
            }
            catch (InvalidOperationException ex) when (ex.Message is "USER_NOT_FOUND" or "BAD_CREDENTIALS")
            {
                return Unauthorized();  // 401
            }
        }
    }
}
