using Application.Repositories;
using Application.Skills;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/skills
    public sealed class SkillsController : ControllerBase
    {
        private readonly ISkillService _svc;
        private readonly ILogger<SkillsController> _logger;

        public SkillsController(ISkillService svc, ILogger<SkillsController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        // GET /api/skills?type=Attack&elementId=2&nameContains=fire&page=1&pageSize=50
        [HttpGet]
        public async Task<IActionResult> List(
           [FromQuery] SkillType? type,
           [FromQuery] int? elementId,
           [FromQuery] string? nameContains,
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 50,
           CancellationToken ct = default)
        {
            var list = await _svc.ListAsync(type, elementId, nameContains, page, pageSize, ct);
            return Ok(list);
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] SkillType? type,
            [FromQuery] int? elementId,
            [FromQuery] bool? isActive,
            [FromQuery] SkillTargetingType? targetingType,
            [FromQuery] TargetSideType? targetSide,
            [FromQuery] AoeShapeType? aoeShape,
            [FromQuery] string? nameContains,
            [FromQuery] SkillSortBy sortBy = SkillSortBy.Name,
            [FromQuery] bool desc = false,
            [FromQuery] string[]? tagsAll = null,
            [FromQuery] string[]? tagsAny = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            // 서비스에 필터형 메서드가 있다면 거기에 맞춰 호출 (필요 시 ISkillService에 추가)
            var filter = new SkillListFilter
            {
                Type = type,
                ElementId = elementId,
                IsActive = isActive,
                TargetingType = targetingType,
                TargetSide = targetSide,
                AoeShape = aoeShape,
                NameContains = nameContains,
                SortBy = sortBy,
                Desc = desc,
                TagsAll = tagsAll,
                TagsAny = tagsAny,
                Skip = Math.Max(0, (page - 1) * pageSize),
                Take = pageSize <= 0 ? 50 : pageSize
            };

            // 아직 서비스에 없다면, 기존 ListAsync만 유지하고 이 엔드포인트는 나중에 연결해도 OK
            return Ok(Array.Empty<SkillListItemDto>());
        }

        // GET /api/skills/123?includeLevels=true
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id, [FromQuery] bool includeLevels = false, CancellationToken ct = default)
        {
            if (includeLevels)
            {
                var dtoWithLv = await _svc.GetWithLevelsAsync(id, ct);
                return dtoWithLv is { } x ? Ok(x) : NotFound();
            }
            else
            {
                var dto = await _svc.GetAsync(id, ct);
                return dto is { } x ? Ok(x) : NotFound();
            }
        }
        // POST /api/skills
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSkillRequest req, CancellationToken ct = default)
        {
            try
            {
                var created = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(Get), new { id = created.SkillId }, created);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Skill failed");
                return Problem("Skill 생성 실패", statusCode: 500);
            }
        }

        // PUT /api/skills/123  (이름/타입/속성/아이콘 등 기본정보 일괄 수정)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSkillBasicsRequest req, CancellationToken ct = default)
        {
            try
            {
                await _svc.UpdateAsync(id, req, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Skill failed");
                return Problem("Skill 수정 실패", statusCode: 500);
            }
        }
        [HttpPut("{id:int}/combat")]
        public async Task<IActionResult> UpdateCombat(int id, [FromBody] UpdateSkillCombatRequest req, CancellationToken ct = default)
        {
            Console.WriteLine($"[API] Combat req: Type={req.Type}, Elem={req.ElementId}, Active={req.IsActive}, Targeting={req.TargetingType}, Aoe={req.AoeShape}, Side={req.TargetSide}");
            await _svc.UpdateCombatAsync(id, req, ct);
            return NoContent();
        }
        [HttpPatch("{id:int}/meta")]
        public async Task<IActionResult> UpdateMeta(int id, [FromBody] PatchSkillMetaRequest req, CancellationToken ct = default)
        {
            Console.WriteLine($"[API] Skill - Create  {req.Tag.Length}");
            Console.WriteLine($"[API] Skill - Create  {req.Tag[0]}");
            Console.WriteLine($"[API] Skill - Create  {req.BaseInfo}");
            try
            {
                await _svc.UpdateMetaAsync(id, req, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Skill Meta failed");
                return Problem("Skill 메타 수정 실패", statusCode: 500);
            }
        }

        // PATCH /api/skills/123/name  (이름만 경량 수정)
        [HttpPatch("{id:int}/name")]
        public async Task<IActionResult> Rename(int id, [FromBody] RenameSkillRequest req, CancellationToken ct = default)
        {
            try
            {
                await _svc.RenameAsync(id, req, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rename Skill failed");
                return Problem("Skill 이름 변경 실패", statusCode: 500);
            }
        }

        // DELETE /api/skills/123
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                await _svc.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Skill failed");
                return Problem("Skill 삭제 실패", statusCode: 500);
            }
        }
    }
}
