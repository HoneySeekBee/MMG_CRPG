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
        const float MoveSpeedPerSec = 10.0f;

        const float PaddingDist = 1.0f;            // 적과의 최소 거리
        const float AllySeparationDist = 1.5f;     // 아군끼리 최소 거리
        const float AllySeparationStrength = 0.75f; // 아군 밀어내는 강도 (0.0 ~ 1.0)
        const float MoveStopRange = 1.0f;
        const float SpawnSnapRange = 0.05f;
        private void UpdateTargetToNearestEnemy(CombatRuntimeState s, ActorState actor)
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
            {
                actor.TargetActorId = FindNearestEnemy(s, actor.ActorId);
            }
        }

        public void Run(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            float speedPerTick = MoveSpeedPerSec * (TickMs / 1000f);

            // 현재 틱에 적이 하나라도 살아 있는지 (현재 웨이브 기준)
            bool anyEnemyAliveThisWave = !s.WaitingNextWave && s.ActiveActors.Values
                .Any(a => a.Team == 1 && !a.Dead && a.Hp > 0 && a.Waveindex == s.CurrentWaveIndex);

            Console.WriteLine($"[Movement] tick: curWave={s.CurrentWaveIndex}, WaitingNextWave={s.WaitingNextWave}, anyEnemyAliveThisWave={anyEnemyAliveThisWave}");

            // 미리 리스트로 캐싱 (foreach 중에 값 변경은 안 하지만, 성능/가독성용)
            var actors = s.ActiveActors.Values.Where(a => !a.Dead && a.Hp > 0).ToList();

            foreach (var actor in actors)
            {
                // 1) 적이 살아있으면: 적에게 다가가되, 아군끼리 조금 벌어지도록
                if (anyEnemyAliveThisWave)
                {
                    UpdateTargetToNearestEnemy(s, actor);

                    if (actor.TargetActorId == null)
                        continue;

                    if (!s.ActiveActors.TryGetValue(actor.TargetActorId.Value, out var target))
                        continue;

                    float dx = target.X - actor.X;
                    float dz = target.Z - actor.Z;
                    float dist = MathF.Sqrt(dx * dx + dz * dz);

                    // 적과의 최소 거리(PaddingDist) 안쪽으로는 너무 파고들지 않게
                    if (dist <= PaddingDist)
                    {
                        // 그래도 살짝은 아군 분리만 적용될 수 있도록
                        ApplyAllySeparation(s, actors, actor, speedPerTick, onlySeparation: true);
                        continue;
                    }

                    Console.WriteLine($"[Move] {actor.ActorId} -> {target.ActorId} dist={dist}");

                    // 기본 타겟 방향
                    float dirX = dx / (dist > 0.0001f ? dist : 1f);
                    float dirZ = dz / (dist > 0.0001f ? dist : 1f);

                    // 아군 분리 벡터 계산
                    (float sepX, float sepZ) = ComputeAllySeparationVector(actors, actor);

                    // 최종 방향 = 타겟 방향 + 분리 방향 * 강도
                    float finalX = dirX + sepX * AllySeparationStrength;
                    float finalZ = dirZ + sepZ * AllySeparationStrength;
                    float finalLen = MathF.Sqrt(finalX * finalX + finalZ * finalZ);

                    if (finalLen > 0.0001f)
                    {
                        finalX /= finalLen;
                        finalZ /= finalLen;

                        actor.X += finalX * speedPerTick;
                        actor.Z += finalZ * speedPerTick;
                    }

                    continue;
                }

                // 2) 적이 아무도 없으면: 플레이어만 Spawn 위치로 복귀
                if (actor.Team != 0)
                    continue;

                if (!actor.ReturningToSpawn)
                    continue;

                float sdx = actor.SpawnX - actor.X;
                float sdz = actor.SpawnZ - actor.Z;
                float sdist = MathF.Sqrt(sdx * sdx + sdz * sdz);

                if (sdist <= SpawnSnapRange)
                {
                    // 거의 도착했다면 정확히 스냅 + 복귀 종료
                    actor.X = actor.SpawnX;
                    actor.Z = actor.SpawnZ;
                    actor.ReturningToSpawn = false;
                    Console.WriteLine($"[Move] {actor.ActorId} spawn reached ({actor.X}, {actor.Z})");
                }
                else
                {
                    float dirX = sdx / (sdist > 0.0001f ? sdist : 1f);
                    float dirZ = sdz / (sdist > 0.0001f ? sdist : 1f);

                    // 복귀 중에도 살짝 아군끼리 벌어지도록 (원하면 빼도 됨)
                    (float sepX, float sepZ) = ComputeAllySeparationVector(actors, actor);

                    float finalX = dirX + sepX * AllySeparationStrength;
                    float finalZ = dirZ + sepZ * AllySeparationStrength;
                    float finalLen = MathF.Sqrt(finalX * finalX + finalZ * finalZ);

                    if (finalLen > 0.0001f)
                    {
                        finalX /= finalLen;
                        finalZ /= finalLen;

                        actor.X += finalX * speedPerTick;
                        actor.Z += finalZ * speedPerTick;
                    }
                }
            }
        }
        private (float, float) ComputeAllySeparationVector(List<ActorState> actors, ActorState self)
        {
            float sepX = 0f;
            float sepZ = 0f;

            foreach (var ally in actors)
            {
                if (ally.ActorId == self.ActorId)
                    continue;

                if (ally.Team != self.Team)
                    continue;

                // 죽은/0hp 아군은 무시
                if (ally.Dead || ally.Hp <= 0)
                    continue;

                float dx = self.X - ally.X;
                float dz = self.Z - ally.Z;
                float dist = MathF.Sqrt(dx * dx + dz * dz);

                if (dist <= 0.0001f)
                    continue;

                // 분리 거리 안에 있을 때만 밀어냄
                if (dist < AllySeparationDist)
                {
                    // 가까울수록 더 강하게 (0 ~ 1)
                    float t = (AllySeparationDist - dist) / AllySeparationDist;

                    // 자신 - 아군 방향으로 밀어내기
                    sepX += (dx / dist) * t;
                    sepZ += (dz / dist) * t;
                }
            }

            return (sepX, sepZ);
        }
        private void ApplyAllySeparation(CombatRuntimeState s, List<ActorState> actors, ActorState actor, float speedPerTick, bool onlySeparation)
        {
            (float sepX, float sepZ) = ComputeAllySeparationVector(actors, actor);
            float len = MathF.Sqrt(sepX * sepX + sepZ * sepZ);

            if (len > 0.0001f)
            {
                sepX /= len;
                sepZ /= len;

                actor.X += sepX * speedPerTick;
                actor.Z += sepZ * speedPerTick;
            }
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