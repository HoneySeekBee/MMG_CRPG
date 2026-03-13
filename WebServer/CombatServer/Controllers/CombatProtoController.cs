using Application.Combat;
using Combat;
using Domain.Enum;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;

namespace CombatServer.Controllers
{
    // Client-facing Protobuf endpoints (mirrors WebServer's GameCombatController)
    [ApiController]
    [Route("api/pb/combat")]
    [Produces("application/x-protobuf")]
    public sealed class CombatProtoController : ControllerBase
    {
        private readonly ICombatService _combat;

        public CombatProtoController(ICombatService combat)
        {
            _combat = combat;
        }

        // POST /api/pb/combat/{combatId}/tick
        [HttpPost("{combatId:long}/tick")]
        public async Task<CombatTickResponsePb> Tick(
            long combatId,
            [FromBody] CombatTickRequestPb req,
            CancellationToken ct)
        {
            var res = await _combat.TickAsync(combatId, req.Tick, ct);
            return MapToPb(res);
        }

        // POST /api/pb/combat/{combatId}/command
        [HttpPost("{combatId:long}/command")]
        public async Task<IActionResult> Command(
            long combatId,
            [FromBody] CombatCommandPb req,
            CancellationToken ct)
        {
            var cmd = new CombatCommandDto(req.ActorId, req.SkillId, req.SkillLevel, req.TargetActorId);
            await _combat.EnqueueCommandAsync(combatId, cmd, ct);
            return Accepted();
        }

        // POST /api/pb/combat/{combatId}/speed/toggle
        [HttpPost("{combatId:long}/speed/toggle")]
        public async Task<ActionResult<ToggleSpeedResponsePb>> ToggleSpeed(
            long combatId,
            [FromBody] ToggleSpeedRequestPb req,
            CancellationToken ct)
        {
            var newSpeed = await _combat.ToggleSpeedAsync(combatId, ct);
            return Ok(new ToggleSpeedResponsePb
            {
                CombatId = combatId,
                Speed = newSpeed switch
                {
                    CombatSpeed.X1  => CombatSpeedPb.CombatSpeedX1,
                    CombatSpeed.X15 => CombatSpeedPb.CombatSpeedX15,
                    CombatSpeed.X2  => CombatSpeedPb.CombatSpeedX2,
                    _               => CombatSpeedPb.CombatSpeedUnspecified
                }
            });
        }

        // GET /api/pb/combat/{combatId}/log
        [HttpGet("{combatId:long}/log")]
        public async Task<CombatLogPagePb> GetLog(
            long combatId,
            [FromQuery] string? cursor,
            [FromQuery] int size = 200,
            CancellationToken ct = default)
        {
            var log = await _combat.GetLogAsync(combatId, cursor, size, ct);

            var pb = new CombatLogPagePb { CombatId = log.CombatId };
            foreach (var e in log.Items)
            {
                var evPb = new CombatLogEventPb { TMs = e.TMs, Type = e.Type, Actor = e.Actor };
                if (e.Target != null) evPb.Target = e.Target;
                if (e.Damage.HasValue) evPb.Damage = e.Damage.Value;
                if (e.Crit.HasValue) evPb.Crit = e.Crit.Value;
                if (e.Extra != null)
                {
                    var s = new Struct();
                    foreach (var kv in e.Extra)
                        s.Fields[kv.Key] = Value.ForString(kv.Value?.ToString() ?? "");
                    evPb.Extra = s;
                }
                pb.Items.Add(evPb);
            }
            if (log.NextCursor != null) pb.NextCursor = log.NextCursor;
            return pb;
        }

        // GET /api/pb/combat/{combatId}/summary
        [HttpGet("{combatId:long}/summary")]
        public async Task<CombatLogSummaryPb> GetSummary(long combatId, CancellationToken ct)
        {
            var summary = await _combat.GetSummaryAsync(combatId, ct);
            return new CombatLogSummaryPb
            {
                CombatId = summary.CombatId,
                TotalEvents = summary.TotalEvents,
                DurationMs = summary.DurationMs,
                DamageDone = summary.DamageDone,
                DamageTaken = summary.DamageTaken
            };
        }

        private static CombatTickResponsePb MapToPb(CombatTickResponse src)
        {
            var snapshotPb = new CombatSnapshotPb();
            foreach (var a in src.Snapshot.Actors)
            {
                snapshotPb.Actors.Add(new ActorSnapshotPb
                {
                    ActorId = a.ActorId,
                    X = a.X,
                    Z = a.Z,
                    Hp = a.Hp,
                    Dead = a.Dead
                });
            }

            var resp = new CombatTickResponsePb
            {
                CombatId = src.CombatId,
                Tick = src.Tick,
                Snapshot = snapshotPb
            };

            foreach (var e in src.Events)
            {
                var evPb = new CombatLogEventPb { TMs = e.TMs, Type = e.Type, Actor = e.Actor };
                if (e.Target != null) evPb.Target = e.Target;
                if (e.Damage.HasValue) evPb.Damage = e.Damage.Value;
                if (e.Crit.HasValue) evPb.Crit = e.Crit.Value;
                if (e.Extra != null)
                {
                    var s = new Struct();
                    foreach (var kv in e.Extra)
                        s.Fields[kv.Key] = Value.ForString(kv.Value?.ToString() ?? "");
                    evPb.Extra = s;
                }
                resp.Events.Add(evPb);
            }
            return resp;
        }
    }
}
