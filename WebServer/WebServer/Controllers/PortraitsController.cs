using Application.Portraits;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // => api/portraits
    public sealed class PortraitsController : ControllerBase
    {
        private readonly PortraitService _svc;
        private readonly IWebHostEnvironment _env;

        public PortraitsController(PortraitService svc, IWebHostEnvironment env)
        {
            _svc = svc;
            _env = env;
        }

        // [1] R - 전체/단건
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _svc.GetAllAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => (await _svc.GetByIdAsync(id, ct) is { } dto ? Ok(dto) : NotFound());

        // [2] C - 생성
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePortraitCommand cmd, CancellationToken ct)
        {
            var dto = await _svc.CreateAsync(cmd, ct);
            return CreatedAtAction(nameof(GetById), new { id = dto.PortraitId }, dto);
        }

        // [3] U - 수정
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePortraitCommand cmd, CancellationToken ct)
        {
            if (cmd.Id != id) return BadRequest();    // route 우선
            var dto = await _svc.UpdateAsync(cmd, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // [4] D - 삭제 (파일도 함께 정리 시도)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var portrait = await _svc.GetByIdAsync(id, ct);
            if (portrait is null) return NotFound();

            var ok = await _svc.DeleteAsync(id, ct);
            if (!ok) return NotFound();

            try
            {
                var root = _env.WebRootPath ?? "wwwroot";
                var path = Path.Combine(root, "portraits", $"{portrait.Key}.png");
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch
            {
                // 로그만 남기고 무시해도 됨
            }

            return NoContent();
        }

        // [5] 업로드 (폼 업로드 권장: key + file)
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] string key, [FromForm] IFormFile file, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(key)) return BadRequest("key is required");
            if (file is null || file.Length == 0) return BadRequest("Empty file");

            await using var stream = file.OpenReadStream();
            var cmd = new UploadPortraitCommand { Key = key, Content = stream, ContentType = file.ContentType };
            await _svc.UploadAsync(cmd, ct);
            return Ok();
        }
    }
}
