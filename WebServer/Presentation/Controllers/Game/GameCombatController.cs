using Application.Combat;
using Combat;
using Domain.Entities;
using Domain.Enum;
using Microsoft.AspNetCore.Mvc;
using WebServer.Mappers;
using static Application.Combat.CombatService;

namespace Presentation.Controllers.Game
{
    [ApiController]
    [Route("api/pb/combat")]
    [Produces("application/x-protobuf")]
    public sealed class GameCombatController : ControllerBase
    {
        private readonly ICombatService _service;

        public GameCombatController(ICombatService service)
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

        // NOTE: Tick / Command / ToggleSpeed / Log / Summary endpoints have been moved to CombatServer.
        // Unity client calls CombatServer directly for these high-frequency requests.

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
                Token = result.Token,
                Result = result.Result switch
                {
                    CombatResult.Win => CombatResultPb.CombatResultWin,
                    CombatResult.Lose => CombatResultPb.CombatResultLose,
                    _ => CombatResultPb.CombatResultUnspecified
                }
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
