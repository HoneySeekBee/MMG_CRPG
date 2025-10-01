using Application.UserCharacter;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/users/{userId:int}/characters")]
    public sealed class UserCharacterController : ControllerBase
    {
        private readonly IUserCharacterService _svc;

        public UserCharacterController(IUserCharacterService svc)
        {
            _svc = svc;
        }

        // 캐릭터 단일 조회
        [HttpGet("{characterId:int}")]
        public async Task<ActionResult<UserCharacterDto>> GetOne(
            int userId,
            int characterId,
            CancellationToken ct = default)
        {
            var dto = await _svc.GetAsync(new GetUserCharacterRequest(userId, characterId), ct);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        // 캐릭터 리스트 조회
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserCharacterDto>>> GetList(
            int userId,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var list = await _svc.GetListAsync(userId, page, pageSize, ct);

            return Ok(list);
        }




        // 캐릭터 생성
        [HttpPost]
        public async Task<ActionResult<UserCharacterDto>> Create(
            int userId,
            [FromBody] CreateUserCharacterRequest body,
            CancellationToken ct = default)
        {
            // body.CharacterId만 사용하고, userId는 경로 기준
            var req = new CreateUserCharacterRequest(userId, body.CharacterId);

            try
            {
                var dto = await _svc.CreateAsync(req, ct);
                return CreatedAtAction(
                    nameof(GetOne),
                    new { userId = dto.UserId, characterId = dto.CharacterId },
                    dto
                );
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); // 이미 보유한 캐릭터
            }
        }

        // 경험치 획득
        [HttpPost("{characterId:int}/gain-exp")]
        public async Task<ActionResult<UserCharacterDto>> GainExp(
            int userId,
            int characterId,
            [FromBody] GainExpRequest body,
            CancellationToken ct = default)
        {
            var req = new GainExpRequest(userId, characterId, body.Amount);
            var dto = await _svc.GainExpAsync(req, ct);
            return Ok(dto);
        }

        // 스킬 학습
        [HttpPost("{characterId:int}/skills/learn")]
        public async Task<ActionResult<UserCharacterDto>> LearnSkill(
            int userId,
            int characterId,
            [FromBody] LearnSkillRequest body,
            CancellationToken ct = default)
        {
            var req = new LearnSkillRequest(userId, characterId, body.SkillId);
            var dto = await _svc.LearnSkillAsync(req, ct);
            return Ok(dto);
        }

        // 스킬 레벨업
        [HttpPost("{characterId:int}/skills/levelup")]
        public async Task<ActionResult<UserCharacterDto>> LevelUpSkill(
            int userId,
            int characterId,
            [FromBody] LevelUpSkillRequest body,
            CancellationToken ct = default)
        {
            var req = new LevelUpSkillRequest(userId, characterId, body.SkillId, body.Amount);
            var dto = await _svc.LevelUpSkillAsync(req, ct);
            return Ok(dto);
        }
    }
}
