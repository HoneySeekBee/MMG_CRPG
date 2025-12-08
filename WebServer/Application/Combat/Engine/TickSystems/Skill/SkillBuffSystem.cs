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
            if (effect.ShieldValue.HasValue)
            {
                ApplyShield(s, caster, target, effect, logs);
                return;
            }
            var existing = target.Buffs
                .FirstOrDefault(b => b.SkillId == effect.SkillId && b.Kind == effect.Kind);

            if (existing != null)
            {
                existing.Stacks = Math.Min(existing.Stacks + 1, effect.MaxStacks);
                existing.DurationMs = effect.DurationMs;

                ApplyStatChange(target, effect.Kind, effect.Value);

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
                Stacks = 1
            };

            target.Buffs.Add(buff);

            // 능력치 반영
            ApplyStatChange(target, effect.Kind, effect.Value);

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
                }));
        }
        private void ApplyShield(
          CombatRuntimeState s,
          ActorState caster,
          ActorState target,
          BuffEffect effect,
          List<CombatLogEventDto> logs)
        {
            int val = effect.ShieldValue!.Value;

            target.Shield += val;
            target.ShieldMax += val;

            target.Buffs.Add(new AppliedBuff
            {
                Kind = BuffKind.Shield,
                SkillId = effect.SkillId,
                Value = val,
                DurationMs = effect.DurationMs,
                MaxDurationMs = effect.DurationMs,
                Stacks = 1
            });

            logs.Add(new CombatLogEventDto(
                s.NowMs(),
                "shield_apply",
                caster.ActorId.ToString(),
                target.ActorId.ToString(),
                val,
                false,
                new Dictionary<string, object?>
                {
                    ["shield"] = target.Shield,
                    ["duration"] = effect.DurationMs
                }
            ));
        }
        private void ApplyStatChange(ActorState target, BuffKind kind, float value)
        {
            switch (kind)
            {
                case BuffKind.AtkUp:
                    target.BuffAtk += (int)value;
                    break;

                case BuffKind.DefUp:
                    target.BuffDef += (int)value;
                    break;

                case BuffKind.CritRateUp:
                    target.BuffCritRate += value;
                    break;

                case BuffKind.CritDamageUp:
                    target.BuffCritDamage += value;
                    break;

                case BuffKind.DamageReduce:
                    target.BuffDamageReduce += value;
                    break;

                case BuffKind.FinalDamageReduce:
                    target.BuffFinalDamageReduce += value;
                    break;

                case BuffKind.DefPenFlat:
                    target.BuffDefPenFlat += (int)value;
                    break;

                case BuffKind.DefPenPercent:
                    target.BuffDefPenPercent += value;
                    break;
            }

            // 즉시 재계산
            target.RecalcStats();
        }
    }
}
