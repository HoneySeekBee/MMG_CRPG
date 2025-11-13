using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public sealed class SnapshotBuilder
    {
        public CombatSnapshotDto Build(CombatRuntimeState s, List<CombatLogEventDto> tickEvents)
        {
            var list = new List<ActorSnapshotDto>();

            foreach (var a in s.Snapshot.Actors.Values)
            {
                var evs = tickEvents
                    .Where(e => e.Actor == a.ActorId.ToString())
                    .ToList();

                list.Add(new ActorSnapshotDto(
                    ActorId: a.ActorId,
                    X: a.X,
                    Z: a.Z,
                    Hp: a.Hp,
                    Dead: a.Dead,
                    Events: evs
                ));
            }

            return new CombatSnapshotDto(list);
        }
    }

}
