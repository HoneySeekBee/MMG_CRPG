using Application.Icons;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IconsController : ControllerBase
    {
        private readonly IconService _svc;
        private readonly IWebHostEnvironment _env;

        public IconsController(IconService svc, IWebHostEnvironment env)
        {
            _svc = svc;
            _env = env;
        }
        #region CRUD 

        // [1] R - 전체 읽기 및 Id로 검색해서 읽기 
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
         => Ok(await _svc.GetAllAsync(ct));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => (await _svc.GetByIdAsync(id, ct) is { } dto ? Ok(dto) : NotFound());

        // [2] C - 생성 
        // Post
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIconCommand cmd, CancellationToken ct)
        {
            var dto = await _svc.CreateAsync(cmd, ct);
            return CreatedAtAction(nameof(GetById), new { id = dto.IconId }, dto);
        }

        // [3] U - 수정 
        // Post
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateIconCommand cmd, CancellationToken ct)
        {
            // route 우선권: 바디의 Id는 무시하고 route id 사용

            if (cmd.Id != id) return BadRequest();

            var dto = await _svc.UpdateAsync(cmd, ct);  // 서비스가 엔티티 로드→필드 적용→저장→IconDto 반환
            return dto is null ? NotFound() : Ok(dto);
        }

        // [4] D - 삭제 
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            // 1) 먼저 조회해서 Key 확보
            var icon = await _svc.GetByIdAsync(id, ct);   // Key, Version 등 응답 DTO
            if (icon is null) return NotFound();

            // 2) DB 삭제 먼저
            var ok = await _svc.DeleteAsync(id, ct);
            if (!ok) return NotFound();                   // 동시삭제 등으로 이미 사라졌을 수도 있음

            // 3) 파일 삭제 (실패해도 서비스는 성공 처리)
            try
            {
                var root = _env.WebRootPath ?? "wwwroot";
                var path = Path.Combine(root, "icons", $"{icon.Key}.png");   // (선택) 설정값으로 대체 가능
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                // TODO: _logger.LogError(ex, "파일 삭제 실패: {Key}", icon.Key);
                // 파일만 남아도 큰 문제는 아니므로 204로 반환
            }

            return NoContent(); // 204
        }
        public sealed class IconUploadRequest
        {
            [Required] public string Key { get; set; } = "";
            [Required] public IFormFile File { get; set; } = default!;
        }
        // 파일 업로드 ( 예 : form-data ) 
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> Upload([FromForm] IconUploadRequest req, CancellationToken ct)
        {
            if (req.File is null || req.File.Length == 0)
                return BadRequest("Empty file");

            await using var stream = req.File.OpenReadStream();
            var cmd = new UploadIconCommand
            {
                Key = req.Key,
                Content = stream,
                ContentType = req.File.ContentType
            };
            await _svc.UploadAsync(cmd, ct);

            return Ok(new { key = req.Key });
        }
        #endregion

    }
}
