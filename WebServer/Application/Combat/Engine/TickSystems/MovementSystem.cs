using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public sealed class MovementSystem
    {
        const float TickMs = 50f;
        const float MoveSpeedPerSec = 10.0f;
        const float MoveStopRange = 1.0f;   
        const float AttackRange = 1.5f;
        public void Run(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            float speedPerTick = MoveSpeedPerSec * (TickMs / 1000f);

            foreach (var actor in s.Snapshot.Actors.Values.Where(a => !a.Dead))
            {
                if (actor.TargetActorId != null)
                {
                    var t = s.Snapshot.Actors[actor.TargetActorId.Value];
                    if (t.Dead)
                        actor.TargetActorId = null;
                }

                if (actor.TargetActorId == null)
                    actor.TargetActorId = FindNearestEnemy(s, actor.ActorId);

                if (actor.TargetActorId == null) continue;
                
                var target = s.Snapshot.Actors[actor.TargetActorId.Value];

                float dx = target.X - actor.X;
                float dz = target.Z - actor.Z;
                float dist = MathF.Sqrt(dx * dx + dz * dz);

                if (dist > MoveStopRange)
                {
                    actor.X += (dx / dist) * speedPerTick;
                    actor.Z += (dz / dist) * speedPerTick;
                }
            }
        }
        private long? FindNearestEnemy(CombatRuntimeState s, long actorId)
        {
            if (!s.Snapshot.Actors.TryGetValue(actorId, out var self))
                return null;

            float nearestDist = float.MaxValue;
            long? nearestId = null;

            foreach (var other in s.Snapshot.Actors.Values)
            {
                // 같은 팀은 제외
                if (other.Team == self.Team) continue;

                // 죽은 적은 제외
                if (other.Dead) continue;

                float dx = other.X - self.X;
                float dz = other.Z - self.Z;
                float dist = MathF.Sqrt(dx * dx + dz * dz);

                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestId = other.ActorId;
                }
            }

            return nearestId;
        }
    }
}
