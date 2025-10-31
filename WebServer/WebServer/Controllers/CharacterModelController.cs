using Application.CharacterModels;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/character-model")]
    public class CharacterModelController : ControllerBase
    {
        private readonly ICharacterModelService _service;

        public CharacterModelController(ICharacterModelService service)
        {
            _service = service;
        }

        [HttpGet("{characterId:int}")]
        public async Task<ActionResult<CharacterModelDto>> Get(int characterId, CancellationToken ct)
        {
            var result = await _service.GetByCharacterIdAsync(characterId, ct);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<CharacterModelDto>>> GetAll(CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateCharacterModelRequest req, CancellationToken ct)
        {
            var id = await _service.CreateAsync(req, ct);
            return Ok(id);
        }

        [HttpPut("{characterId:int}")]
        public async Task<IActionResult> Update(int characterId, [FromBody] CreateCharacterModelRequest req, CancellationToken ct)
        {
            await _service.UpdateAsync(characterId, req, ct);
            return NoContent();
        }

        [HttpDelete("{characterId:int}")]
        public async Task<IActionResult> Delete(int characterId, CancellationToken ct)
        {
            await _service.DeleteAsync(characterId, ct);
            return NoContent();
        }
    }
}
