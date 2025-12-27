using Domain.Combat.Runtime;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Combat.Engine.TickSystems
{
    public class ProjectileSystem
    { 
        public void Run(CombatRuntimeState s, List<CombatLogEvent> logs, int dtMs) 
        {
            if (dtMs <= 0) return;

            float dt = dtMs / 1000f;
            var removeList = new List<ProjectileState>();

            foreach (var p in s.Projectiles)
            {
                // 1) lifetime
                p.LifetimeMs -= dtMs;
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

        private void HandleAoeHit(CombatRuntimeState s, List<CombatLogEvent> logs, ProjectileState p)
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
                    if (!s.ActiveActors.TryGetValue(p.CasterId, out var caster) || caster.Dead)
                        continue;
                    s.PendingSkillCasts.Enqueue(new PendingSkillCast
                    {
                        CasterId = p.CasterId,
                        SkillId = p.SkillId,
                        TargetActorIds = new List<long> { actor.ActorId },
                        HitIndex = 0,
                        ExtraMultiplier = 1.0f,
                        DelayMs = 0
                    });

                    logs.Add(new CombatLogEvent(
                        s.NowMs,
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
             List<CombatLogEvent> logs,
             ProjectileState p,
             ActorState target)
        {
            if (!s.ActiveActors.TryGetValue(p.CasterId, out var caster) || caster.Dead)
                return;

            s.PendingSkillCasts.Enqueue(new PendingSkillCast
            {
                CasterId = p.CasterId,
                SkillId = p.SkillId,
                TargetActorIds = new List<long> { target.ActorId },
                HitIndex = 0,
                ExtraMultiplier = 1.0f,
                DelayMs = 0
            });

            logs.Add(new CombatLogEvent(
                s.NowMs,
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
