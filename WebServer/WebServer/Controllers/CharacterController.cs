using Application.Character;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/characters")]
    public sealed class CharacterController : ControllerBase
    {

        private readonly ICharacterService _svc;

        public CharacterController(ICharacterService svc) => _svc = svc;

        // GET /api/characters
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CharacterSummaryDto>), 200)]
        public async Task<ActionResult<PagedResult<CharacterSummaryDto>>> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? elementId = null,
            [FromQuery] int? rarityId = null,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var query = new CharacterListQuery(page, pageSize, elementId, rarityId, search);
            var result = await _svc.GetListAsync(query, ct);
            return Ok(result);
        }

        // GET /api/characters/{id}

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                var dto = await _svc.GetDetailAsync(id, ct);
                if (dto is null) return NotFound();
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return Problem(
                    statusCode: 500,
                    title: "Get character failed",
                    detail: ex.Message
                );
            }
        }

        // POST /api/characters
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateCharacterRequest req, CancellationToken ct = default)
        {
            Console.WriteLine($"[API] [Character] [Create] | iconId :{req.IconId} Portrait : {req.PortraitId} element : {req.ElementId}, role : {req.RoleId}");
            try
            {
                var id = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(nameof(GetById), new { id }, new { id });
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
        }

        // PUT /api/characters/{id}
        [HttpPut("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateBasic(int id, [FromBody] UpdateCharacterRequest req, CancellationToken ct = default)
        {
            try
            {
                await _svc.UpdateBasicAsync(id, req, ct);
                return NoContent();
            }
            catch (InvalidOperationException) // not found
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
        }

        // PUT /api/characters/{id}/skills
        [HttpPut("{id:int}/skills")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SetSkills(int id, [FromBody] IReadOnlyList<UpsertSkillRequest> req, CancellationToken ct = default)
        {
            try
            {
                // 존재 여부 확인용(없으면 404)
                var detail = await _svc.GetDetailAsync(id, ct);
                if (detail is null) return NotFound();

                await _svc.SetSkillsAsync(id, req, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
        }

        // PUT /api/characters/{id}/progressions
        [HttpPut("{id:int}/progressions")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SetProgressions(int id, [FromBody] IReadOnlyList<UpsertProgressionRequest> req, CancellationToken ct = default)
        {
            try
            {
                var detail = await _svc.GetDetailAsync(id, ct);
                if (detail is null) return NotFound();

                await _svc.SetProgressionsAsync(id, req, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
        }

        // PUT /api/characters/{id}/promotions
        [HttpPut("{id:int}/promotions")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SetPromotions(int id, [FromBody] IReadOnlyList<UpsertPromotionRequest> req, CancellationToken ct = default)
        {
            try
            {
                var detail = await _svc.GetDetailAsync(id, ct);
                if (detail is null) return NotFound();

                await _svc.SetPromotionsAsync(id, req, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return ValidationProblem(detail: ex.Message, statusCode: 400);
            }
        }

        // DELETE /api/characters/{id}
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                await _svc.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }
    }
}
