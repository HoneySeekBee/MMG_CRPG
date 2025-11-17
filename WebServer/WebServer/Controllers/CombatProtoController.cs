using Application.Combat;
using Combat;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using WebServer.Mappers;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/combat")]
    [Produces("application/x-protobuf")]
    public sealed class CombatProtoController : ControllerBase
    {
        private readonly ICombatService _service;

        public CombatProtoController(ICombatService service)
        {
            _service = service;
        }

        // START
        [HttpPost("start")]
        public async Task<ActionResult<StartCombatResponsePb>> Start(
            [FromBody] StartCombatRequestPb req, CancellationToken ct)
        {
            var domainReq = new StartCombatRequest(req.StageId, req.UserId, req.BattleId);
            var res = await _service.StartAsync(domainReq, ct);
            return CombatProtoMapper.ToPb(res);
        }

        // COMMAND
        [HttpPost("{combatId:long}/command")]
        public async Task<IActionResult> Command(
            long combatId,
            [FromBody] CombatCommandPb req,
            CancellationToken ct)
        {
            var domainCmd = CombatProtoMapper.ToDomain(req);
            await _service.EnqueueCommandAsync(combatId, domainCmd, ct);
            return Accepted();
        }

        // LOG
        [HttpGet("{combatId:long}/log")]
        public async Task<CombatLogPagePb> GetLog(
            long combatId,
            string? cursor,
            int size = 200,
            CancellationToken ct = default)
        {
            var log = await _service.GetLogAsync(combatId, cursor, size, ct);
            return CombatProtoMapper.ToPb(log);
        }

        //SUMMARY
        [HttpGet("{combatId:long}/summary")]
        public async Task<CombatLogSummaryPb> GetSummary(
            long combatId,
            CancellationToken ct)
        {
            var summary = await _service.GetSummaryAsync(combatId, ct);
            return CombatProtoMapper.ToPb(summary);
        }

        [HttpPost("{combatId:long}/tick")]
        public async Task<CombatTickResponsePb> Tick(long combatId, [FromBody] CombatTickRequestPb req, CancellationToken ct)
        {
            var res = await _service.TickAsync(combatId, req.Tick, ct);
            return CombatProtoMapper.ToPb(res);
        }

        [HttpPost("{combatId:long}/finish")]
        public async Task<ActionResult<FinishCombatResponsePb>> Finish([FromRoute] long combatId, [FromBody] FinishCombatRequestPb req, CancellationToken ct)
        {
            if (req.CombatId == 0)
            {
                req.CombatId = combatId;
            }
            else if (req.CombatId != combatId)
            {
                return BadRequest("COMBAT_ID_MISMATCH");
            }

            var appReq = new FinishCombatRequest(
                CombatId: req.CombatId,
                UserId: req.UserId
            );

            var result = await _service.FinishAsync(appReq, ct);

            var pb = new FinishCombatResponsePb
            {
                StageId = result.StageId,
                Stars = (int)result.Stars,       // enum -> int
                FirstClear = result.FirstClear,
                Gold = result.Gold,
                Gem = result.Gem,
                Token = result.Token
            };

            pb.Rewards.AddRange(result.Items.Select(i => new StageRewardItemPb
            {
                ItemId = i.ItemId,
                Qty = i.Qty,
                FirstClearReward = i.IsFirstClearReward
            }));

            return Ok(pb);
        }
    }
}
