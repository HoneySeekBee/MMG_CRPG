using Application.SkillLevels;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/skills/{skillId:int}/levels")]
    public sealed class SkillLevelsController : ControllerBase
    {
        private readonly ISkillLevelService _svc;
        private readonly ILogger<SkillLevelsController> _logger;

        public SkillLevelsController(ISkillLevelService svc, ILogger<SkillLevelsController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        // GET /api/skills/{skillId}/levels
        [HttpGet]
        public async Task<IActionResult> List(int skillId, CancellationToken ct = default)
            => Ok(await _svc.ListAsync(skillId, ct));

        // GET /api/skills/{skillId}/levels/{level}
        [HttpGet("{level:int}")]
        public async Task<IActionResult> Get(int skillId, int level, CancellationToken ct = default)
            => (await _svc.GetAsync(skillId, level, ct)) is { } dto ? Ok(dto) : NotFound();

        // POST /api/skills/{skillId}/levels
        [HttpPost]
        public async Task<IActionResult> Create(int skillId, [FromBody] CreateSkillLevelRequest req, CancellationToken ct = default)
        {
            Console.WriteLine($"[WebAPI : SkillLevelCreate] id = {skillId}, level = {req.Level}. values = {req.Values}, material = {req.Materials}");
            try
            {
                var created = await _svc.CreateAsync(skillId, req, ct);
                // 생성된 리소스 위치: /api/skills/{skillId}/levels/{level}
                return CreatedAtAction(nameof(Get), new { skillId, level = created.Level }, created);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create SkillLevel failed");
                return Problem("SkillLevel 생성 실패", statusCode: 500);
            }
        }

        // PUT /api/skills/{skillId}/levels/{level}
        [HttpPut("{level:int}")]
        public async Task<IActionResult> Update(int skillId, int level, [FromBody] UpdateSkillLevelRequest req, CancellationToken ct = default)
        {
            try
            {
                await _svc.UpdateAsync(skillId, level, req, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update SkillLevel failed");
                return Problem("SkillLevel 수정 실패", statusCode: 500);
            }
        }

        // DELETE /api/skills/{skillId}/levels/{level}
        [HttpDelete("{level:int}")]
        public async Task<IActionResult> Delete(int skillId, int level, CancellationToken ct = default)
        {
            try
            {
                await _svc.DeleteAsync(skillId, level, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete SkillLevel failed");
                return Problem("SkillLevel 삭제 실패", statusCode: 500);
            }
        }
    }
}
