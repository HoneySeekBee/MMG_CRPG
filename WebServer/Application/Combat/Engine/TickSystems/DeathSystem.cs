using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public sealed class DeathSystem
    {
        public void Run(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            foreach (var a in s.Snapshot.Actors.Values)
            {
                if (a.Hp <= 0 && !a.Dead)
                {
                    evs.Add(new CombatLogEventDto(
                        TMs: NowMs(s),
                        Type: "dead",
                        Actor: a.ActorId.ToString(),
                        Target: null,
                        Damage: null,
                        Crit: null,
                        Extra: null
                    ));
                }
            }
        }

        private int NowMs(CombatRuntimeState s)
            => (int)(DateTimeOffset.UtcNow - s.StartedAt).TotalMilliseconds;
    }
}
