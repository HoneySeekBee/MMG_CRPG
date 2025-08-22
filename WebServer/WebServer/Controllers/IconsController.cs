using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebServer.Models;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IconsController : ControllerBase
    {
        private readonly GameDBContext _db;
        public IconsController(GameDBContext db)
        {
            _db = db;
        }

        #region CRUD 

        // [1] R - 전체 읽기 및 Id로 검색해서 읽기 
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var Icons = await _db.Icons.AsNoTracking().ToListAsync(ct);
            return Ok(Icons);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var icon = await _db.Icons
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IconId == id, ct);

            if (icon is null)
                return NotFound();

            return Ok(icon);
        }

        // [2] C - 생성 
        // Post
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Icon icon, CancellationToken ct)
        {
            _db.Icons.Add(icon);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                return Conflict(new { message = $"Failed Create Icon... {ex.Message} " });
            }
            return CreatedAtAction(nameof(GetById), new {id = icon.IconId}, icon);
        }

        // [3] U - 수정 
        // Post
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] Icon updateIcon, CancellationToken ct)
        {
            var icon = await _db.Icons.FirstOrDefaultAsync(i => i.IconId == id, ct);
            if (icon is null)
                return NotFound();

            icon.Key = updateIcon.Key;
            icon.Path = updateIcon.Path;
            icon.Atlas = updateIcon.Atlas;
            icon.X = updateIcon.X;
            icon.Y = updateIcon.Y;
            icon.W = updateIcon.W;
            icon.H = updateIcon.H;
            icon.Version = updateIcon.Version;

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch(DbUpdateException ex)
            {
                return Conflict(new { message = "Failed Icon Update", detail = ex.Message });
            }
            return Ok(icon);
        }

        // [4] D - 삭제 

        // Post
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfiremd(int id, CancellationToken ct)
        {
            var icon = await _db.Icons.FirstOrDefaultAsync(i => i.IconId == id);
            if (icon is null)
                return NotFound();

            _db.Icons.Remove(icon);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        #endregion

    }
}
