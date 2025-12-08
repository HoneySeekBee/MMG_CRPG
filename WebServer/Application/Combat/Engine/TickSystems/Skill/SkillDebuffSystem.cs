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
    public class SkillDebuffSystem
    {
        public void Apply(
        CombatRuntimeState s,
        ActorState caster,
        ActorState target,
        DebuffEffect effect,
        List<CombatLogEventDto> logs)
        {
            // ===== BLEED =====
            if (effect.BleedDamage.HasValue && effect.BleedDuration.HasValue)
            {
                target.Buffs.Add(new AppliedBuff
                {
                    Kind = BuffKind.Bleed,
                    SkillId = effect.SkillId,
                    Value = effect.BleedDamage.Value,
                    DurationMs = effect.BleedDuration.Value * 1000,
                    MaxDurationMs = effect.BleedDuration.Value * 1000,
                    Stacks = 1
                });

                logs.Add(new CombatLogEventDto(
                    s.NowMs(),
                    "bleed_apply",
                    caster.ActorId.ToString(),
                    target.ActorId.ToString(),
                    effect.BleedDamage.Value,
                    null,
                    new Dictionary<string, object?>
                    {
                        ["duration"] = effect.BleedDuration
                    }
                ));
            }

            if (effect.DelayedMultiplier.HasValue && effect.DelayedDelaySec.HasValue)
            {
                s.PendingSkillCasts.Enqueue(new PendingSkillCast
                {
                    CasterId = caster.ActorId,
                    TargetId = target.ActorId,
                    SkillId = effect.SkillId,
                    SkillLevel = 0,
                    DelayMs = (int)(effect.DelayedDelaySec.Value * 1000),
                    ExtraMultiplier = effect.DelayedMultiplier.Value
                });

                logs.Add(new CombatLogEventDto(
                    s.NowMs(),
                    "delayed_hit_scheduled",
                    caster.ActorId.ToString(),
                    target.ActorId.ToString(),
                    null,
                    null,
                    new Dictionary<string, object?>
                    {
                        ["multiplier"] = effect.DelayedMultiplier,
                        ["delaySec"] = effect.DelayedDelaySec
                    }
                ));
            }
        }
    }
}
