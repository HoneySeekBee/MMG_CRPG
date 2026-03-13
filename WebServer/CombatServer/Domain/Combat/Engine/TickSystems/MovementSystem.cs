using Domain.Combat.Runtime;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Combat.Engine.TickSystems
{
    public sealed class MovementSystem
    {
        const float MoveSpeedPerSec = 20.0f;

        const float AllySeparationDist = 1.5f;
        const float AllySeparationStrength = 0.5f;
        const float SpawnSnapRange = 0.05f;

        private const float CollisionRadius = 1.2f;
        private const float EnemyRadius = 1.6f;
        public void Run(CombatRuntimeState s, List<CombatLogEvent> evs, int dtMs)
        {
            float dtSeconds = dtMs / 1000f;
            float speedPerTick = MoveSpeedPerSec * dtSeconds;

            bool anyEnemyAlive = s.ActiveActors.Values.Any(a =>
     a.Team == 1 && !a.Dead && a.Hp > 0 && a.Waveindex == s.CurrentWaveIndex);

            var actors = s.ActiveActors.Values
                .Where(a => !a.Dead && a.Hp > 0)
                .ToList();

            foreach (var actor in actors)
            {
                if (actor.IsKnockbacked)
                {
                    HandleKnockback(actor, dtMs);
                    continue; // 원래 이동 시스템 무시
                }
                if (actor.Stunned || actor.Frozen || actor.KnockedDown || actor.Rooted)
                {
                    continue;
                }
                if (anyEnemyAlive && !s.WaitingNextWave)
                    HandleCombatMovement(s, actors, actor, speedPerTick);
                else
                    HandleReturnToSpawn(s, actors, actor, speedPerTick);
            }
        }
        private void HandleKnockback(ActorState actor, int dtMs)
        {
            float dtSeconds = dtMs / 1000f;

            actor.X += actor.KnockbackVX * dtSeconds;
            actor.Z += actor.KnockbackVZ * dtSeconds;

            actor.KnockbackRemainMs -= dtMs;

            if (actor.KnockbackRemainMs <= 0)
            {
                actor.IsKnockbacked = false;
                actor.KnockbackVX = 0;
                actor.KnockbackVZ = 0;
                actor.KnockbackRemainMs = 0;
            }
        }

        private void HandleCombatMovement(CombatRuntimeState s, List<ActorState> actors, ActorState actor, float speedPerTick)
        {
            UpdateTarget(s, actor);

            if (actor.Stunned || actor.Frozen || actor.KnockedDown || actor.Rooted) return;
            if (actor.TargetActorId == null) return;
            if (!s.ActiveActors.TryGetValue(actor.TargetActorId.Value, out var target)) return;

            float dx = target.X - actor.X;
            float dz = target.Z - actor.Z;
            float dist = MathF.Sqrt(dx * dx + dz * dz);

            float stopRange = actor.RangeBase - 0.01f;
            float minCollisionDist = CollisionRadius + EnemyRadius;

            // facing은 항상 타겟 기준으로 유지
            actor.FacingX = dx / (dist + 0.0001f);
            actor.FacingZ = dz / (dist + 0.0001f);

            // 너무 붙었으면 살짝만 떼기
            if (dist < minCollisionDist)
                ApplyEnemyRepulsionSmall(actor, dx, dz, dist, minCollisionDist, speedPerTick);

            if (dist <= stopRange)
            {
                return;
            }

            float dirX = dx / (dist + 0.0001f);
            float dirZ = dz / (dist + 0.0001f);

            // overshoot 방지: 사거리 경계까지만
            float moveDist = dist - stopRange;
            float step = MathF.Min(speedPerTick, moveDist);

            actor.X += dirX * step;
            actor.Z += dirZ * step;

        }
        private void ApplyEnemyRepulsionSmall(ActorState actor, float dx, float dz, float dist, float minDist, float maxStep)
        {
            if (dist < 0.001f) return;

            float overlap = (minDist - dist);
            if (overlap <= 0f) return;

            float push = overlap * 0.3f;
            push = MathF.Min(push, 0.05f);
            push = MathF.Min(push, maxStep * 0.2f);

            float dirX = dx / dist;
            float dirZ = dz / dist;

            actor.X -= dirX * push;
            actor.Z -= dirZ * push;
        }

        private void HandleReturnToSpawn(CombatRuntimeState s, List<ActorState> actors, ActorState actor, float speedPerTick)
        {
            if (actor.Team != 0 || !actor.ReturningToSpawn)
                return;

            float dx = actor.SpawnX - actor.X;
            float dz = actor.SpawnZ - actor.Z;
            float dist = MathF.Sqrt(dx * dx + dz * dz);

            if (dist < SpawnSnapRange)
            {
                actor.X = actor.SpawnX;
                actor.Z = actor.SpawnZ;
                actor.ReturningToSpawn = false;
                actor.ArrivedAtSpawn = true;
                return;
            }

            // dist=0 보호
            if (dist < 0.0001f)
            {
                actor.X = actor.SpawnX;
                actor.Z = actor.SpawnZ;
                actor.ReturningToSpawn = false;
                actor.ArrivedAtSpawn = true;
                return;
            }

            float dirX = dx / dist;
            float dirZ = dz / dist;

            float step = MathF.Min(speedPerTick, dist); // clamp
            actor.X += dirX * step;
            actor.Z += dirZ * step;

            // 이동 후 스냅(보험)
            float ndx = actor.SpawnX - actor.X;
            float ndz = actor.SpawnZ - actor.Z;
            float ndist = MathF.Sqrt(ndx * ndx + ndz * ndz);

            if (ndist < SpawnSnapRange)
            {
                actor.X = actor.SpawnX;
                actor.Z = actor.SpawnZ;
                actor.ReturningToSpawn = false;
                actor.ArrivedAtSpawn = true;
            }
        }

        private void UpdateTarget(CombatRuntimeState s, ActorState actor)
        {
            if (actor.TargetActorId != null)
            {
                if (!s.ActiveActors.TryGetValue(actor.TargetActorId.Value, out var t) ||
                    t.Dead || t.Hp <= 0)
                {
                    actor.TargetActorId = null;
                }
            }

            if (actor.TargetActorId == null)
                actor.TargetActorId = FindNearestEnemy(s, actor.ActorId);
        }

        private long? FindNearestEnemy(CombatRuntimeState s, long actorId)
        {
            if (!s.ActiveActors.TryGetValue(actorId, out var self))
                return null;

            float nearestDist = float.MaxValue;
            long? nearestId = null;

            foreach (var other in s.ActiveActors.Values)
            {
                if (other.Team == self.Team) continue;
                if (other.Dead || other.Hp <= 0) continue;

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
