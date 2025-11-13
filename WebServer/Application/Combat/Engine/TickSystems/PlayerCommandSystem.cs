using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public sealed class PlayerCommandSystem
    {
        public void Run(CombatRuntimeState state, List<CombatLogEventDto> evs)
        {
            while (state.PendingCommands.Count > 0)
            {
                var cmd = state.PendingCommands.Dequeue();
                var actor = state.Snapshot.Actors[cmd.ActorId];

                if (actor.SkillCooldownMs > 0)
                    continue;

                actor.TargetActorId = cmd.TargetActorId;
                actor.SkillCooldownMs = 3000;

                evs.Add(new CombatLogEventDto(
                    TMs: NowMs(state),
                    Type: "skill_cast",
                    Actor: actor.ActorId.ToString(),
                    Target: cmd.TargetActorId?.ToString(),
                    Damage: null,
                    Crit: null,
                    Extra: new Dictionary<string, object?> { ["skillId"] = cmd.SkillId }
                ));
            }
        }

        private int NowMs(CombatRuntimeState s)
            => (int)(DateTimeOffset.UtcNow - s.StartedAt).TotalMilliseconds;
    }
}
