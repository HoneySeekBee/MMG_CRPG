using Domain.Combat.Runtime;
using Domain.Enum;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Combat.Engine.TickSystems
{
    public sealed class DeathSystem
    {
        public void Run(CombatRuntimeState s, List<CombatLogEvent> evs)
        {
            int now = s.NowMs;

            foreach (var a in s.ActiveActors.Values)
            {
                if (a.Hp <= 0 && !a.Dead)
                {
                    a.Hp = 0;
                    a.Dead = true;

                    evs.Add(new CombatLogEvent(
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
                s.Result = CombatResult.Lose;
                s.Phase = CombatBattlePhase.Completed;

                evs.Add(new CombatLogEvent(
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
