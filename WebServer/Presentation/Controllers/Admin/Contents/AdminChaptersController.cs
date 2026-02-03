using Application.Contents.Chapters;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Admin.Contents
{
    [ApiController]
    [Route("api/chapters")]
    public class AdminChaptersController : ControllerBase
    {
        private readonly IChapterService _chapterService;

        public AdminChaptersController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        // GET: api/chapters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChapterDto>>> GetList(CancellationToken cancellationToken)
        {
            var chapters = await _chapterService.GetListAsync(cancellationToken);
            return Ok(chapters);
        }

        // GET: api/chapters/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ChapterDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var chapter = await _chapterService.GetByIdAsync(id, cancellationToken);
            if (chapter is null)
                return NotFound();

            return Ok(chapter);
        }

        // POST: api/chapters
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateChapterRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _chapterService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        // PUT: api/chapters/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateChapterRequest request, CancellationToken cancellationToken)
        {
            if (id != request.ChapterId)
                return BadRequest("id mismatch");

            var ok = await _chapterService.UpdateAsync(request, cancellationToken);
            if (!ok)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/chapters/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var ok = await _chapterService.DeleteAsync(id, cancellationToken);
            if (!ok)
                return NotFound();

            return NoContent();
        }
    }
}
