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
        const float TickMs = 25f;
        const float MoveSpeedPerSec = 20.0f;

        const float AllySeparationDist = 1.5f;
        const float AllySeparationStrength = 0.5f;
        const float SpawnSnapRange = 0.05f;

        private const float CollisionRadius = 1.2f;
        private const float EnemyRadius = 1.6f;

        public void Run(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            float speedPerTick = MoveSpeedPerSec * (TickMs / 1000f);

            bool anyEnemyAlive = !s.WaitingNextWave &&
                s.ActiveActors.Values.Any(a =>
                    a.Team == 1 &&
                    !a.Dead &&
                    a.Hp > 0 &&
                    a.Waveindex == s.CurrentWaveIndex);

            var actors = s.ActiveActors.Values
                .Where(a => !a.Dead && a.Hp > 0)
                .ToList();

            foreach (var actor in actors)
            {
                if (anyEnemyAlive)
                    HandleCombatMovement(s, actors, actor, speedPerTick);
                else
                    HandleReturnToSpawn(s, actors, actor, speedPerTick);
            }
        }


        private void HandleCombatMovement(CombatRuntimeState s, List<ActorState> actors, ActorState actor, float speedPerTick)
        {
            UpdateTarget(s, actor);
            if (actor.TargetActorId == null)
                return;

            if (!s.ActiveActors.TryGetValue(actor.TargetActorId.Value, out var target))
                return;

            float dx = target.X - actor.X;
            float dz = target.Z - actor.Z;
            float dist = MathF.Sqrt(dx * dx + dz * dz);

            float stopRange = actor.Range;
            float minCollisionDist = CollisionRadius + EnemyRadius;

            // 1) 적과 겹침 → 내 유닛만 뒤로 빼기
            if (dist < minCollisionDist)
            {
                ApplyEnemyRepulsion(actor, dx, dz, dist, minCollisionDist);
                return;
            }

            // 2) 사거리 안이면 이동 금지
            if (dist < stopRange)
                return;

            // 3) 정상 이동 (아군 방향 보정만 적용)
            float dirX = dx / dist;
            float dirZ = dz / dist;

            (float sepAX, float sepAZ) = ComputeAllySeparation(actors, actor);
            (float sepEX, float sepEZ) = ComputeEnemySeparation(s, actor);

            // 아군 분리는 방향에만 영향 (위치 이동 없음)
            float finalX = dirX + sepAX * AllySeparationStrength;
            float finalZ = dirZ + sepAZ * AllySeparationStrength;

            float len = MathF.Sqrt(finalX * finalX + finalZ * finalZ);
            if (len > 0.0001f)
            {
                finalX /= len;
                finalZ /= len;

                actor.X += finalX * speedPerTick;
                actor.Z += finalZ * speedPerTick;
            }
        }

        private void ApplyEnemyRepulsion(ActorState actor, float dx, float dz, float dist, float minDist)
        {
            if (dist < 0.001f) return;

            float push = (minDist - dist);

            float dirX = dx / dist;
            float dirZ = dz / dist;

            actor.X -= dirX * push * 0.8f;
            actor.Z -= dirZ * push * 0.8f;
        }


        // 아군은 실제 이동시키지 않음 → 방향 보정만 사용
        private (float, float) ComputeAllySeparation(List<ActorState> actors, ActorState self)
        {
            float sepX = 0f;
            float sepZ = 0f;

            foreach (var ally in actors)
            {
                if (ally.ActorId == self.ActorId) continue;
                if (ally.Team != self.Team) continue;

                float dx = self.X - ally.X;
                float dz = self.Z - ally.Z;
                float dist = MathF.Sqrt(dx * dx + dz * dz);

                if (dist >= AllySeparationDist || dist < 0.001f) continue;

                float t = (AllySeparationDist - dist) / AllySeparationDist;

                sepX += (dx / dist) * t;
                sepZ += (dz / dist) * t;
            }

            return (sepX, sepZ);
        }


        private (float, float) ComputeEnemySeparation(CombatRuntimeState s, ActorState self)
        {
            float sepX = 0f;
            float sepZ = 0f;

            foreach (var enemy in s.ActiveActors.Values)
            {
                if (enemy.Team == self.Team) continue;

                float dx = self.X - enemy.X;
                float dz = self.Z - enemy.Z;
                float dist = MathF.Sqrt(dx * dx + dz * dz);

                float minDist = CollisionRadius + EnemyRadius;

                if (dist >= minDist || dist < 0.001f) continue;

                float t = (minDist - dist) / minDist;
                sepX += (dx / dist) * t;
                sepZ += (dz / dist) * t;
            }

            return (sepX, sepZ);
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
                return;
            }

            float dirX = dx / dist;
            float dirZ = dz / dist;

            (float sepAX, float sepAZ) = ComputeAllySeparation(actors, actor);

            float finalX = dirX + sepAX * 0.5f;
            float finalZ = dirZ + sepAZ * 0.5f;

            float len = MathF.Sqrt(finalX * finalX + finalZ * finalZ);
            if (len > 0.0001f)
            {
                finalX /= len;
                finalZ /= len;

                actor.X += finalX * speedPerTick;
                actor.Z += finalZ * speedPerTick;
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