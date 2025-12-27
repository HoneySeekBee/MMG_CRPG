using Domain.Combat.Runtime;
using Domain.Entities.Combats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Combat.Engine.TickSystems
{
    public sealed class SnapshotBuilder
    {
        public CombatSnapshot Build(CombatRuntimeState s)
        {
            var snapshot = new CombatSnapshot();
            foreach (var actor in s.ActiveActors.Values)
            {
                var aSnap = new ActorSnapshot
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
