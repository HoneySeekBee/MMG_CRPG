using Amazon.Runtime.Internal;
using Application.Storage;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/image")]
    public class ImageController : ControllerBase
    {
        private readonly IIconStorage _storage;
        private readonly IPortraitStorage _portraitStorage;
        private readonly IConnectionMultiplexer _redis;
        public ImageController(IIconStorage storage, IPortraitStorage portraitStorage, IConnectionMultiplexer redis)
        {
            _storage = storage;
            _portraitStorage = portraitStorage;
            _redis = redis;
        }

        [HttpGet("icons/{key}")]
        public async Task<IActionResult> GetIcon(string key, CancellationToken ct)
        {
            // 1) Redis 세션 체크
            var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized();

            var db = _redis.GetDatabase();
            var session = await db.StringGetAsync($"session:{sessionId}");
            if (session.IsNullOrEmpty)
                return Unauthorized();

            // 2) S3에서 파일 읽기
            var bytes = await _storage.LoadAsync(key, ct);

            // 3) 반환
            return File(bytes, "image/png");
        }
        [HttpGet("portraits/{key}")]
        public async Task<IActionResult> GetPortrait(string key, CancellationToken ct)
        {
            // 1) Redis 세션 체크
            var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized();

            var db = _redis.GetDatabase();
            var session = await db.StringGetAsync($"session:{sessionId}");
            if (session.IsNullOrEmpty)
                return Unauthorized();

            // 2) S3에서 파일 읽기
            var bytes = await _portraitStorage.LoadAsync(key, ct);

            // 3) 반환
            return File(bytes, "image/png");
        }
    }
}
