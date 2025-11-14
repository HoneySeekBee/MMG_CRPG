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
        public CombatSnapshotDto Build(CombatRuntimeState s)
        {
            var snapshot = new CombatSnapshotDto();
            foreach (var actor in s.ActiveActors.Values)
            {
                var aSnap = new ActorSnapshotDto
                {
                    ActorId = actor.ActorId,
                    X = actor.X,
                    Z = actor.Z,
                    Hp = actor.Hp,
                    Dead = actor.Dead,
                };
                snapshot.Actors.Add(aSnap);
            }
            return snapshot;
        }
    }

}
