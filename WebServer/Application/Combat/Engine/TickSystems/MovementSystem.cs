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

            // 현재 틱에 적이 하나라도 살아 있는지
            bool anyEnemyAliveThisWave = !s.WaitingNextWave && s.ActiveActors.Values
          .Any(a => a.Team == 1 && !a.Dead && a.Hp > 0 && a.Waveindex == s.CurrentWaveIndex);

            Console.WriteLine($"[Movement] anyEnemyAliveThisWave={anyEnemyAliveThisWave}, WaitingNextWave={s.WaitingNextWave}");

            foreach (var actor in s.ActiveActors.Values.Where(a => !a.Dead && a.Hp > 0))
            {
                // 1) 적이 살아있으면 기존 로직 유지 (적에게 이동)
                if (anyEnemyAliveThisWave)
                {
                    UpdateTargetToNearestEnemy(s, actor);

                    if (actor.TargetActorId == null)
                        continue;

                    var target = s.ActiveActors[actor.TargetActorId.Value];

                    float dx = target.X - actor.X;
                    float dz = target.Z - actor.Z;
                    float dist = MathF.Sqrt(dx * dx + dz * dz);

                    Console.WriteLine($"[Move] {actor.ActorId} -> {target.ActorId} dist={dist}");

                    if (dist > MoveStopRange)
                    {
                        actor.X += (dx / dist) * speedPerTick;
                        actor.Z += (dz / dist) * speedPerTick;
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
                    actor.X += (sdx / sdist) * speedPerTick;
                    actor.Z += (sdz / sdist) * speedPerTick;
                    Console.WriteLine($"[Move] {actor.ActorId} -> spawn dist={sdist}");
                }
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