using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public class ProjectileSystem
    {
        private const int TickMs = 100; // 서버 틱 기준 맞춰라

        public void Run(CombatRuntimeState s, List<CombatLogEventDto> logs)
        {
            var removeList = new List<ProjectileState>();

            foreach (var p in s.Projectiles)
            {
                // 1) lifetime
                p.LifetimeMs -= TickMs;
                if (p.LifetimeMs <= 0)
                {
                    removeList.Add(p);
                    continue;
                }

                // 2) tracking
                if (p.Tracking && p.TargetId.HasValue)
                {
                    if (s.ActiveActors.TryGetValue(p.TargetId.Value, out var t) && !t.Dead)
                    {
                        float dx = t.X - p.X;
                        float dz = t.Z - p.Z;
                        float len = MathF.Sqrt(dx * dx + dz * dz);

                        if (len > 0.001f)
                        {
                            dx /= len;
                            dz /= len;
                            p.VX = dx * p.Speed;
                            p.VZ = dz * p.Speed;
                        }
                    }
                }

                // 3) move
                float dt = TickMs / 1000f;
                p.X += p.VX * dt;
                p.Z += p.VZ * dt;

                // 4) collision
                foreach (var actor in s.ActiveActors.Values)
                {
                    if (actor.ActorId == p.CasterId) continue;
                    if (actor.Dead) continue;

                    // 이미 맞은 대상이면 무시 (중복타 방지)
                    if (p.HitActors.Contains(actor.ActorId)) continue;

                    float dx = actor.X - p.X;
                    float dz = actor.Z - p.Z;
                    float dist = MathF.Sqrt(dx * dx + dz * dz);

                    if (dist < 0.7f)
                    {
                        // 기록
                        p.HitActors.Add(actor.ActorId);

                        // AOE hit
                        if (p.AoeRadius > 0)
                            HandleAoeHit(s, logs, p);
                        else
                            HandleSingleHit(s, logs, p, actor);

                        // hit count 끝났으면 삭제
                        if (p.HitActors.Count >= p.MaxHitCount)
                        {
                            removeList.Add(p);
                            break;
                        }

                        // piercing이 false면 삭제
                        if (!p.Piercing)
                        {
                            removeList.Add(p);
                            break;
                        }
                    }
                }
            }

            // remove
            foreach (var p in removeList)
                s.Projectiles.Remove(p);
        }

        private void HandleAoeHit(CombatRuntimeState s, List<CombatLogEventDto> logs, ProjectileState p)
        {
            foreach (var actor in s.ActiveActors.Values)
            {
                if (actor.Dead) continue;
                if (actor.ActorId == p.CasterId) continue;

                float dx = actor.X - p.X;
                float dz = actor.Z - p.Z;
                float dist = MathF.Sqrt(dx * dx + dz * dz);

                if (dist <= p.AoeRadius)
                {
                    // 이미 때린 적이면 스킵
                    if (p.HitActors.Contains(actor.ActorId)) continue;
                    p.HitActors.Add(actor.ActorId);

                    s.PendingSkillCasts.Enqueue(new PendingSkillCast
                    {
                        CasterId = p.CasterId,
                        SkillId = p.SkillId,
                        TargetActorIds = new List<long> { actor.ActorId },
                        HitIndex = 0,
                        ExtraMultiplier = 1.0f,
                        DelayMs = 0
                    });

                    logs.Add(new CombatLogEventDto(
                        s.NowMs(),
                        "projectile_aoe_hit",
                        p.CasterId.ToString(),
                        actor.ActorId.ToString(),
                        null,
                        null,
                        new Dictionary<string, object?>
                        {
                            ["skillId"] = p.SkillId,
                            ["radius"] = p.AoeRadius
                        }
                    ));
                }
            }
        }

        private void HandleSingleHit(
             CombatRuntimeState s,
             List<CombatLogEventDto> logs,
             ProjectileState p,
             ActorState target)
        {
            s.PendingSkillCasts.Enqueue(new PendingSkillCast
            {
                CasterId = p.CasterId,
                SkillId = p.SkillId,
                TargetActorIds = new List<long> { target.ActorId },
                HitIndex = 0,
                ExtraMultiplier = 1.0f,
                DelayMs = 0
            });

            logs.Add(new CombatLogEventDto(
                s.NowMs(),
                "projectile_hit",
                p.CasterId.ToString(),
                target.ActorId.ToString(),
                null,
                null,
                new Dictionary<string, object?>
                {
                    ["skillId"] = p.SkillId
                }
            ));
        }
    }
}
