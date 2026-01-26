using Amazon.Runtime.Internal;
using Application.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Presentation.Controllers.Admin
{
    [ApiController]
    [Route("api/image")]
    public class AdminImageController : ControllerBase
    {
        private readonly IIconStorage _storage;
        private readonly IPortraitStorage _portraitStorage;
        private readonly IConnectionMultiplexer _redis;

        public AdminImageController(IIconStorage storage, IPortraitStorage portraitStorage, IConnectionMultiplexer redis)
        {
            _storage = storage;
            _portraitStorage = portraitStorage;
            _redis = redis;
        }

        private async Task<bool> IsAuthorizedAsync(CancellationToken ct)
        {
            // [1] Unity 방식: X-Session-Id
            var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(sessionId))
            {
                var db = _redis.GetDatabase();
                var session = await db.StringGetAsync($"session:{sessionId}");
                if (!session.IsNullOrEmpty) return true;
            }

            // [2] AdminTool 방식: JWT Bearer (검증 강제 실행)
            var auth = await HttpContext.AuthenticateAsync(); // 기본 스킴(Bearer)로 인증 시도
            return auth.Succeeded;
        }

        [HttpGet("icons/{key}")]
        public async Task<IActionResult> GetIcon(string key, CancellationToken ct)
        {
            if (!await IsAuthorizedAsync(ct))
                return Unauthorized();

            var bytes = await _storage.LoadAsync(key, ct);
            return File(bytes, "image/png");
        }

        [HttpGet("portraits/{key}")]
        public async Task<IActionResult> GetPortrait(string key, CancellationToken ct)
        {
            if (!await IsAuthorizedAsync(ct))
                return Unauthorized();

            var bytes = await _portraitStorage.LoadAsync(key, ct);
            return File(bytes, "image/png");
        }
    }
}
