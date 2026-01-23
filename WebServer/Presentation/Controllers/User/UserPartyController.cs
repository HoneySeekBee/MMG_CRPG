using Application.Rarities;
using Application.Repositories;
using Application.UserParties;
using Domain.Entities.User;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers.User
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPartyController : ControllerBase
    {
        private readonly IUserPartyRepository _repo;
        private readonly ILogger<UserPartyController> _logger;

        public UserPartyController(IUserPartyRepository repo, ILogger<UserPartyController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // GET: api/userparty/{partyId}
        [HttpGet("{partyId:long}")]
        public async Task<ActionResult<UserParty>> GetById(long partyId, CancellationToken ct)
        {
            var party = await _repo.GetByIdAsync(partyId, ct);
            if (party == null)
                return NotFound();

            return Ok(party);
        }

        // GET: api/userparty/by-battle?userId=1&battleId=10
        [HttpGet("by-battle")]
        public async Task<ActionResult<UserParty>> GetByUserBattle([FromQuery] int userId, [FromQuery] int battleId, CancellationToken ct)
        {
            var party = await _repo.GetByUserBattleAsync(userId, battleId, ct);
            if (party == null)
                return NotFound();

            return Ok(party);
        }

        // POST: api/userparty 
        [HttpPost]
        public async Task<ActionResult<long>> Create([FromBody] CreateUserPartyRequest req, CancellationToken ct)
        {
            var exists = await _repo.ExistsAsync(req.UserId, req.BattleId, ct);
            if (exists)
                return Conflict("Party already exists for this battle.");

            var partyId = await _repo.CreateAsync(req.UserId, req.BattleId, req.SlotCount, ct);
            return CreatedAtAction(nameof(GetById), new { partyId }, partyId);
        }

        // PUT: api/userparty/{partyId}/assign
        [HttpPut("{partyId:long}/assign")]
        public async Task<IActionResult> Assign(long partyId, [FromBody] AssignCharacterRequest req, CancellationToken ct)
        {
            var party = await _repo.GetByIdAsync(partyId, ct);
            if (party == null)
                return NotFound();

            party.Assign(req.SlotId, req.UserCharacterId);
            await _repo.SaveAsync(party, ct);

            return NoContent();
        }

        // PUT: api/userparty/{partyId}/unassign
        [HttpPut("{partyId:long}/unassign")]
        public async Task<IActionResult> Unassign(long partyId, [FromBody] UnassignCharacterRequest req, CancellationToken ct)
        {
            var party = await _repo.GetByIdAsync(partyId, ct);
            if (party == null)
                return NotFound();

            party.Unassign(req.SlotId);
            await _repo.SaveAsync(party, ct);

            return NoContent();
        }

        // PUT: api/userparty/{partyId}/swap
        [HttpPut("{partyId:long}/swap")]
        public async Task<IActionResult> Swap([FromBody] SwapSlotsRequest req, CancellationToken ct)
        {
            var party = await _repo.GetByIdAsync(req.PartyId, ct);
            if (party == null)
                return NotFound();

            party.Swap(req.SlotA, req.SlotB);
            await _repo.SaveAsync(party, ct);
            return NoContent();
        }
    }
}
