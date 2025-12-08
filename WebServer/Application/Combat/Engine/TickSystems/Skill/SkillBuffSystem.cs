using Application.Combat.Runtime;
using Domain.Entities.Skill;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems.Skill
{
    public class SkillBuffSystem
    {
        public void Apply(
        CombatRuntimeState s,
        ActorState caster,
        ActorState target,
        BuffEffect effect,
        List<CombatLogEventDto> logs)
        {
            var existing = target.Buffs
                .FirstOrDefault(b => b.SkillId == effect.SkillId && b.Kind == effect.Kind);

            if (existing != null)
            {
                existing.Stacks = Math.Min(existing.Stacks + 1, effect.MaxStacks);
                existing.DurationMs = effect.DurationMs;

                logs.Add(new CombatLogEventDto(
                    s.NowMs(),
                    "buff_refresh",
                    caster.ActorId.ToString(),
                    target.ActorId.ToString(),
                    null,
                    null,
                    new Dictionary<string, object?>
                    {
                        ["kind"] = effect.Kind.ToString(),
                        ["value"] = effect.Value,
                        ["stacks"] = existing.Stacks,
                        ["duration"] = effect.DurationMs
                    }
                ));

                return;
            }

            var buff = new AppliedBuff
            {
                Kind = effect.Kind,
                SkillId = effect.SkillId,
                Value = effect.Value,
                MaxDurationMs = effect.DurationMs,
                DurationMs = effect.DurationMs,
                Stacks = 1,
            };

            target.Buffs.Add(buff);

            logs.Add(new CombatLogEventDto(
                s.NowMs(),
                "buff_apply",
                caster.ActorId.ToString(),
                target.ActorId.ToString(),
                null,
                null,
                new Dictionary<string, object?>
                {
                    ["kind"] = effect.Kind.ToString(),
                    ["value"] = effect.Value,
                    ["duration"] = effect.DurationMs
                }
            ));
        }
    }
}
