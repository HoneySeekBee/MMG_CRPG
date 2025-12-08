using Application.Combat.Runtime;
using Application.Repositories;
using Application.SkillLevels;
using Application.Skills;
using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems.Skill
{

    public class SkillSystem
    {
        private readonly SkillResolver _resolver = new();
        public void Run(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            int count = s.PendingSkillCasts.Count;

            while (count-- > 0)
            {
                var cast = s.PendingSkillCasts.Dequeue();

                // 딜레이 처리
                if (cast.DelayMs > 0)
                {
                    cast.DelayMs -= 100; // tick = 100ms 라고 가정
                    if (cast.DelayMs > 0)
                    {
                        s.PendingSkillCasts.Enqueue(cast); // 다시 큐에 넣음
                        continue;
                    }
                }

                ExecuteSkill(s, evs, cast);
            }
        }

        private void ExecuteSkill(CombatRuntimeState state, List<CombatLogEventDto> evs, PendingSkillCast cast)
        {
            if (!state.ActiveActors.TryGetValue(cast.CasterId, out var caster))
                return;
            if (caster.Stunned || caster.Frozen || caster.KnockedDown)
                return;

            var skill = state.SkillMaster[cast.SkillId];
            if (caster.Silenced && skill.Type != SkillType.Attack)
                return;

            var effect = skill.Effect;
            // 투사체 처리
            if (skill.BaseInfo?["projectile"] != null)
            {
                SpawnProjectile(state, caster, cast, skill);
                return;
            }
            // 즉발형 광역기 
            if (skill.BaseInfo?["aoe"] != null)
            {
                float radius = skill.BaseInfo["aoe"]!["radius"]!.GetValue<float>();

                float cx = caster.X;
                float cz = caster.Z;

                var targets = state.ActiveActors.Values
                    .Where(a => !a.Dead)
                    .Where(a => a.Team != caster.Team)
                    .Where(a =>
                    {
                        float dx = a.X - cx;
                        float dz = a.Z - cz;
                        return MathF.Sqrt(dx * dx + dz * dz) <= radius;
                    })
                    .ToList();

                foreach (var t in targets)
                {
                    _resolver.Execute(
                        state, caster, t,
                        effect, evs,
                        0,
                        1.0f
                    );

                    evs.Add(new CombatLogEventDto(
                        state.NowMs(),
                        "skill_hit_aoe",
                        caster.ActorId.ToString(),
                        t.ActorId.ToString(),
                        null,
                        null,
                        new Dictionary<string, object?>
                        {
                            ["skillId"] = cast.SkillId,
                            ["radius"] = radius
                        }
                    ));
                }

                return; // AoE는 여기서 종료
            }

            int totalHits = skill.BaseInfo?["hits"]?.GetValue<int>() ?? 1;
            int currentHit = cast.HitIndex;

            if (currentHit == 0 && cast.TargetActorIds.Count == 0)
            {
                var initialTargets = TargetSelector.SelectTargets(state, caster, skill);
                cast.TargetActorIds = initialTargets.Select(t => t.ActorId).ToList();
            }

            var aliveTargets = cast.TargetActorIds
                .Select(id => state.ActiveActors.TryGetValue(id, out var t) ? t : null)
                .Where(t => t != null && !t.Dead)
                .ToList();

            if (aliveTargets.Count == 0)
                return;

            foreach (var t in aliveTargets)
            {
                _resolver.Execute(
                    state, caster, t!,
                    effect, evs,
                    currentHit,
                    cast.ExtraMultiplier
                );

                evs.Add(new CombatLogEventDto(
                    state.NowMs(),
                    "skill_hit",
                    caster.ActorId.ToString(),
                    t.ActorId.ToString(),
                    null,
                    null,
                    new Dictionary<string, object?>
                    {
                        ["skillId"] = cast.SkillId,
                        ["hit"] = currentHit + 1
                    }
                ));
            }

            // 다음 hit 예약
            if (currentHit + 1 < totalHits)
            {
                state.PendingSkillCasts.Enqueue(new PendingSkillCast
                {
                    CasterId = cast.CasterId,
                    TargetActorIds = cast.TargetActorIds,
                    SkillId = cast.SkillId,
                    HitIndex = currentHit + 1,
                    ExtraMultiplier = 1.0f,
                    DelayMs = 100
                });
                return;
            }

            // 딜레이 히트
            var delayed = skill.BaseInfo?["extra"]?["delayedHit"];
            if (delayed != null)
            {
                float delay = delayed["delay"]!.GetValue<float>();
                float multiplier = delayed["multiplier"]!.GetValue<float>();

                state.PendingSkillCasts.Enqueue(new PendingSkillCast
                {
                    CasterId = cast.CasterId,
                    TargetActorIds = cast.TargetActorIds,
                    SkillId = cast.SkillId,
                    HitIndex = -1,
                    ExtraMultiplier = multiplier,
                    DelayMs = (int)(delay * 1000)
                });
            }
        } 
        private long? AutoSelectTarget(CombatRuntimeState s, ActorState caster, SkillWithLevelsDto skill)
        {
            var actors = s.ActiveActors.Values;

            return skill.TargetSide switch
            {
                TargetSideType.Enemy =>
                    actors.Where(a => a.Team != caster.Team && !a.Dead)
                          .OrderBy(a => Distance(caster, a))
                          .Select(a => a.ActorId)
                          .FirstOrDefault(),

                TargetSideType.Team =>
                    actors.Where(a => a.Team == caster.Team && !a.Dead)
                          .OrderBy(a => (float)a.Hp / a.HpMax)
                          .Select(a => a.ActorId)
                          .FirstOrDefault(),

                _ => null
            };
        }
        private float Distance(ActorState a, ActorState b)
        {
            float dx = a.X - b.X;
            float dz = a.Z - b.Z;
            return MathF.Sqrt(dx * dx + dz * dz);
        }

        private int NowMs(CombatRuntimeState s)
        {
            return (int)(DateTimeOffset.UtcNow - s.StartedAt).TotalMilliseconds;
        }
        private void SpawnProjectile(
     CombatRuntimeState s,
     ActorState caster,
     PendingSkillCast cast,
     SkillWithLevelsDto skill)
        {
            var pInfo = skill.BaseInfo!["projectile"]!;
            float speed = pInfo["speed"].GetValue<float>();
            int lifetime = pInfo["lifetime"].GetValue<int>();
            bool tracking = pInfo["tracking"]?.GetValue<bool>() ?? false;

            bool piercing = pInfo["pierce"]?.GetValue<bool>() ?? false;
            float aoeRadius = pInfo["aoeRadius"]?.GetValue<float>() ?? 0f;
            int maxHit = pInfo["maxHit"]?.GetValue<int>() ?? 1;

            // 타겟 선택
            var targets = TargetSelector.SelectTargets(s, caster, skill);
            if (targets.Count == 0)
                return;

            var target = targets[0];

            // 방향 계산
            float dx = target.X - caster.X;
            float dz = target.Z - caster.Z;
            float len = MathF.Sqrt(dx * dx + dz * dz);

            if (len > 0.001f)
            {
                dx /= len;
                dz /= len;
            }

            // 발사체 등록
            s.Projectiles.Add(new ProjectileState
            {
                Id = s.Projectiles.Count + 1,
                CasterId = caster.ActorId,
                SkillId = skill.SkillId,

                X = caster.X,
                Z = caster.Z,

                VX = dx * speed,
                VZ = dz * speed,

                Speed = speed,
                LifetimeMs = lifetime,

                Tracking = tracking,
                TargetId = tracking ? target.ActorId : null,

                // 신규 옵션 반영
                Piercing = piercing,
                AoeRadius = aoeRadius,
                MaxHitCount = maxHit,

                Effect = skill.Effect
            });
        }
    }
}
