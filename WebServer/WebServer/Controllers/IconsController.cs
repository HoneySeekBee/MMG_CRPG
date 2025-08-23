using Application.Icons;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IconsController : ControllerBase
    {
        private readonly IconService _svc;

        public IconsController(IconService svc)
            => _svc = svc;

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
            if (cmd.Id != id) return BadRequest();
            var dto = await _svc.UpdateAsync(cmd, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        // [4] D - 삭제 

        // Post
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
         => await _svc.DeleteAsync(id, ct) ? NoContent() : NotFound();

        // 파일 업로드 ( 예 : form-data ) 
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] string key, [FromForm] IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0) return BadRequest("Empty file");

            await using var stream = file.OpenReadStream();
            var cmd = new UploadIconCommand { Key = key, Content = stream, ContentType = file.ContentType };
            await _svc.UploadAsync(cmd, ct);
            return Ok();
        }
        #endregion

    }
}
