using Application.Combat.Engine.TickSystems.Skill;
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

namespace Application.Combat.Engine.TickSystems
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

            var skill = state.SkillMaster[cast.SkillId];

            // === 자동 타겟팅 적용 ===
            long? targetId = cast.TargetId;
            if (targetId == null)
                targetId = AutoSelectTarget(state, caster, skill);

            if (targetId == null || !state.ActiveActors.TryGetValue(targetId.Value, out var target))
                return;

            var effect = skill.Effect;

            // === ExtraMultiplier 처리 ===
            if (effect.Damage != null && cast.ExtraMultiplier != 1.0f)
            {
                effect = effect with
                {
                    Damage = effect.Damage with
                    {
                        Multiplier = effect.Damage.Multiplier * cast.ExtraMultiplier
                    }
                };
            }

            // 스킬 실행
            _resolver.Execute(state, caster, target, effect, evs);

            evs.Add(new CombatLogEventDto(
                state.NowMs(),
                "skill_execute",
                caster.ActorId.ToString(),
                target.ActorId.ToString(),
                null,
                null,
                new Dictionary<string, object?>
                {
                    ["skillId"] = cast.SkillId,
                    ["level"] = cast.SkillLevel
                }
            ));
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
    }
}
