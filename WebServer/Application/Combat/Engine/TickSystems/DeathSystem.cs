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
            int now = s.NowMs;

            foreach (var a in s.ActiveActors.Values)
            {
                if (a.Hp <= 0 && !a.Dead)
                {
                    a.Hp = 0;
                    a.Dead = true;

                    evs.Add(new CombatLogEventDto(
                        TMs: now,
                        Type: "dead",
                        Actor: a.ActorId.ToString(),
                        Target: null,
                        Damage: null,
                        Crit: null,
                        Extra: null
                    ));
                }
            }

            if (s.BattleEnded)
                return;

            var alive = s.ActiveActors.Values.Where(a => !a.Dead && a.Hp > 0).ToList();
            if (!alive.Any())
                return;

            bool anyPlayerAlive = alive.Any(a => a.Team == 0);

            if (!anyPlayerAlive)
            {
                s.BattleEnded = true;

                evs.Add(new CombatLogEventDto(
                    TMs: now,
                    Type: "stage_result",
                    Actor: "",
                    Target: "",
                    Damage: null,
                    Crit: null,
                    Extra: new Dictionary<string, object?> { ["result"] = "lose" }
                ));
            }
        } 
    }
}
