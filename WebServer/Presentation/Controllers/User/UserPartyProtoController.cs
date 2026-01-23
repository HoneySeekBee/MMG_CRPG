using Application.Repositories;
using Application.UserParties;
using Microsoft.AspNetCore.Mvc;
using Contracts.Assets;
using Contracts.UserParty;
using WebServer.Mappers;
using Google.Protobuf.WellKnownTypes;

namespace WebServer.Controllers.User
{
    [ApiController]
    [Route("api/pb/userparty")]
    [Produces("application/x-protobuf")]
    [Consumes("application/x-protobuf")]

    public class UserPartyProtoController : ControllerBase
    {
        private readonly IUserPartyRepository _repo;
        private readonly ILogger<UserPartyProtoController> _logger;

        public UserPartyProtoController(IUserPartyRepository repo, ILogger<UserPartyProtoController> logger)
        {
            _repo = repo;
            _logger = logger;
        }
        // POST api/pb/userparty/create
        [HttpPost("create")]
        public async Task<ActionResult<CreateUserPartyResponsePb>> Create([FromBody] CreateUserPartyRequestPb req, CancellationToken ct)
        {
            if (req.SlotCount <= 0)
                return BadRequest();

            var exists = await _repo.ExistsAsync(req.UserId, req.BattleId, ct);
            if (exists) return Conflict();

            var id = await _repo.CreateAsync(req.UserId, req.BattleId, req.SlotCount, ct);
            return CreatedAtAction(nameof(GetById), new { partyId = id },
                new CreateUserPartyResponsePb { PartyId = id });
        }

        // GET api/pb/userparty/{partyId}
        [HttpGet("{partyId:long}")]
        public async Task<ActionResult<GetUserPartyResponsePb>> GetById(long partyId, CancellationToken ct)
        {
            var party = await _repo.GetByIdAsync(partyId, ct);
            if (party == null) return NotFound();

            return Ok(party.ToGetResponse());
        }

        // GET api/pb/userparty/by-battle?userId=1&battleId=2
        [HttpGet("by-battle")]
        public async Task<ActionResult<GetUserPartyResponsePb>> GetByUserBattle([FromQuery] int userId, [FromQuery] int battleId, CancellationToken ct)
        {
            var party = await _repo.GetByUserBattleAsync(userId, battleId, ct);
            if (party == null)
            { 
                const int defaultSlotCount = 10;

                var newId = await _repo.CreateAsync(userId, battleId, defaultSlotCount, ct);
                var created = await _repo.GetByIdAsync(newId, ct);

                return Ok(created.ToGetResponse());
            }
            return Ok(party.ToGetResponse());
        }

        // PUT api/pb/userparty/assign
        [HttpPut("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignCharacterRequestPb req, CancellationToken ct)
        {
            var party = await _repo.GetByIdAsync(req.PartyId, ct);
            if (party == null) return NotFound();

            party.Assign(req.SlotId, req.UserCharacterId);
            await _repo.SaveAsync(party, ct);
            return Ok(new Empty());
        }

        // PUT api/pb/userparty/unassign
        [HttpPut("unassign")]
        public async Task<IActionResult> Unassign([FromBody] UnassignCharacterRequestPb req, CancellationToken ct)
        {
            var party = await _repo.GetByIdAsync(req.PartyId, ct);
            if (party == null) return NotFound();

            party.Unassign(req.SlotId);
            await _repo.SaveAsync(party, ct);
            return Ok(new Empty());
        }

        // PUT api/pb/userparty/swap
        [HttpPut("swap")]
        public async Task<IActionResult> Swap([FromBody] SwapSlotsRequestPb req, CancellationToken ct)
        {
            var party = await _repo.GetByIdAsync(req.PartyId, ct);
            if (party == null) return NotFound();

            party.Swap(req.SlotA, req.SlotB);
            await _repo.SaveAsync(party, ct);
            return Ok(new Empty());
        }

        [HttpPut("bulk-assign")]
        public async Task<IActionResult> BulkAssign([FromBody] BulkAssignRequestPb req, CancellationToken ct)
        {
            var party = await _repo.GetByIdAsync(req.PartyId, ct);
            if (party == null) return NotFound();
            foreach (var p in party.Slots)
            {
                party.Unassign(p.SlotId);
            } 
            foreach (var p in req.Pairs)
            {
                var v = p.UserCharacterId;
                if (v != null && v.Value != 0)
                {
                    party.Assign(p.SlotId, v.Value);
                }
            }
            await _repo.SaveAsync(party, ct);
            return Ok(new Empty());
        }
    }
}
