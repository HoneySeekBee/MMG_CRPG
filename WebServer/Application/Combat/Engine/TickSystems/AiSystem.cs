using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public sealed class AiSystem
    {
        public void Run(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            foreach (var actor in s.Snapshot.Actors.Values.Where(a => !a.Dead))
            {
                if (actor.TargetActorId == null)
                    actor.TargetActorId = FindNearestEnemy(s, actor.ActorId);
            }
        }

        private long? FindNearestEnemy(CombatRuntimeState s, long actorId)
        {
            var self = s.Snapshot.Actors[actorId];

            return s.Snapshot.Actors.Values
                .Where(a => a.Team != self.Team && !a.Dead)
                .OrderBy(a => Distance(self, a))
                .FirstOrDefault()?.ActorId;
        }

        private float Distance(ActorState a, ActorState b)
        {
            float dx = a.X - b.X;
            float dz = a.Z - b.Z;
            return MathF.Sqrt(dx * dx + dz * dz);
        }
    }
}
