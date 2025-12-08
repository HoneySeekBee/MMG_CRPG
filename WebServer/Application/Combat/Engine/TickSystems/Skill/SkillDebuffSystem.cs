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
        CombatRuntimeState state,
        ActorState caster,
        ActorState target,
        DebuffEffect effect,
        List<CombatLogEventDto> logs,
        int hitIndex,
        float extraMultiplier)
        {
            if (target.ImmuneDebuff)
            { 
                Log(state, logs, "immune_all", caster, target);
                return;
            }
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
            }
            // Delayed Hit
            if (effect.DelayedMultiplier.HasValue && effect.DelayedDelaySec.HasValue)
            {
                state.PendingSkillCasts.Enqueue(new PendingSkillCast
                {
                    CasterId = caster.ActorId,
                    TargetActorIds = new List<long> { target.ActorId },
                    SkillId = effect.SkillId,
                    HitIndex = -1,
                    ExtraMultiplier = effect.DelayedMultiplier.Value,
                    DelayMs = (int)(effect.DelayedDelaySec.Value * 1000)
                });
            }
            // CC effects
            ApplyStun(state, caster, target, effect, logs);
            ApplySilence(state, caster, target, effect, logs);
            ApplyFreeze(state, caster, target, effect, logs);
            ApplyRoot(state, caster, target, effect, logs);
            ApplyKnockdown(state, caster, target, effect, logs);

            ApplyKnockback(state, caster, target, effect, logs);
            ApplyPull(state, caster, target, effect, logs);
            ApplyPush(state, caster, target, effect, logs);
        }
        private void ApplyStun(
         CombatRuntimeState state, ActorState caster, ActorState target,
         DebuffEffect effect, List<CombatLogEventDto> logs)
        {
            if (!effect.StunDurationMs.HasValue) return;

            if (target.ImmuneStun)
            {
                Log(state, logs, "immune_stun", caster, target);
                return;
            }

            if (Random.Shared.NextDouble() < target.StunResistChance)
            {
                Log(state, logs, "resist_stun", caster, target);
                return;
            }

            int dur = effect.StunDurationMs.Value;
            dur = (int)(dur * (1f - target.StunDurationReduce)); // 지속시간 감소

            target.Stunned = true;
            target.StunMs = dur;

            Log(state, logs, "apply_stun", caster, target, new() { ["duration"] = dur });
        }

        private void ApplySilence(
            CombatRuntimeState state, ActorState caster, ActorState target,
            DebuffEffect effect, List<CombatLogEventDto> logs)
        {
            if (!effect.SilenceDurationMs.HasValue) return;

            if (target.ImmuneSilence)
            {
                Log(state, logs, "immune_silence", caster, target);
                return;
            }

            int dur = effect.SilenceDurationMs.Value;
            dur = (int)(dur * (1f - target.SilenceDurationReduce));

            target.Silenced = true;
            target.SilenceMs = dur;

            Log(state, logs, "apply_silence", caster, target, new() { ["duration"] = dur });
        }

        private void ApplyFreeze(
            CombatRuntimeState state, ActorState caster, ActorState target,
            DebuffEffect effect, List<CombatLogEventDto> logs)
        {
            if (!effect.FreezeDurationMs.HasValue) return;

            if (target.ImmuneFreeze)
            {
                Log(state, logs, "immune_freeze", caster, target);
                return;
            }

            int dur = effect.FreezeDurationMs.Value;
            dur = (int)(dur * (1f - target.FreezeDurationReduce));

            target.Frozen = true;
            target.FreezeMs = dur;

            Log(state, logs, "apply_freeze", caster, target, new() { ["duration"] = dur });
        }

        private void ApplyRoot(
            CombatRuntimeState state, ActorState caster, ActorState target,
            DebuffEffect effect, List<CombatLogEventDto> logs)
        {
            if (!effect.RootDurationMs.HasValue) return;

            if (target.ImmuneRoot)
            {
                Log(state, logs, "immune_root", caster, target);
                return;
            }

            int dur = effect.RootDurationMs.Value;
            dur = (int)(dur * (1f - target.RootDurationReduce));

            target.Rooted = true;
            target.RootMs = dur;

            Log(state, logs, "apply_root", caster, target, new() { ["duration"] = dur });
        }

        private void ApplyKnockdown(
            CombatRuntimeState state, ActorState caster, ActorState target,
            DebuffEffect effect, List<CombatLogEventDto> logs)
        {
            if (!effect.KnockdownDurationMs.HasValue) return;

            if (target.ImmuneKnockdown)
            {
                Log(state, logs, "immune_knockdown", caster, target);
                return;
            }

            int dur = effect.KnockdownDurationMs.Value;

            target.KnockedDown = true;
            target.KnockdownMs = dur;

            Log(state, logs, "apply_knockdown", caster, target, new() { ["duration"] = dur });
        }
         
        // Knockback / Pull / Push 

        private void ApplyKnockback(
            CombatRuntimeState state, ActorState caster, ActorState target,
            DebuffEffect effect, List<CombatLogEventDto> logs)
        {
            if (!effect.KnockbackDistance.HasValue || !effect.KnockbackSpeed.HasValue)
                return;

            if (target.ImmuneKnockback)
            {
                Log(state, logs, "immune_knockback", caster, target);
                return;
            }

            float dist = effect.KnockbackDistance.Value;
            float speed = effect.KnockbackSpeed.Value;

            float dx = target.X - caster.X;
            float dz = target.Z - caster.Z;

            float len = MathF.Sqrt(dx * dx + dz * dz);
            if (len > 0.0001f)
            {
                dx /= len;
                dz /= len;
            }

            target.IsKnockbacked = true;
            target.KnockbackVX = dx * speed;
            target.KnockbackVZ = dz * speed;
            target.KnockbackRemainMs = (int)((dist / speed) * 1000f);

            Log(state, logs, "knockback", caster, target);
        }

        private void ApplyPull(
            CombatRuntimeState state, ActorState caster, ActorState target,
            DebuffEffect effect, List<CombatLogEventDto> logs)
        {
            if (!effect.PullDistance.HasValue || !effect.PullSpeed.HasValue)
                return;

            float dist = effect.PullDistance.Value;
            float speed = effect.PullSpeed.Value;

            float dx = caster.X - target.X;
            float dz = caster.Z - target.Z;

            float len = MathF.Sqrt(dx * dx + dz * dz);
            if (len > 0.0001f)
            {
                dx /= len;
                dz /= len;
            }

            target.IsKnockbacked = true;
            target.KnockbackVX = dx * speed;
            target.KnockbackVZ = dz * speed;
            target.KnockbackRemainMs = (int)((dist / speed) * 1000f);

            Log(state, logs, "pull", caster, target);
        }

        private void ApplyPush(
            CombatRuntimeState state, ActorState caster, ActorState target,
            DebuffEffect effect, List<CombatLogEventDto> logs)
        {
            if (!effect.PushDistance.HasValue ||
                !effect.PushSpeed.HasValue ||
                !effect.PushDirX.HasValue ||
                !effect.PushDirZ.HasValue)
                return;

            float dx = effect.PushDirX.Value;
            float dz = effect.PushDirZ.Value;

            float len = MathF.Sqrt(dx * dx + dz * dz);
            if (len > 0.0001f)
            {
                dx /= len;
                dz /= len;
            }

            float dist = effect.PushDistance.Value;
            float speed = effect.PushSpeed.Value;

            target.IsKnockbacked = true;
            target.KnockbackVX = dx * speed;
            target.KnockbackVZ = dz * speed;
            target.KnockbackRemainMs = (int)((dist / speed) * 1000f);

            Log(state, logs, "push", caster, target);
        }

        private void Log(
            CombatRuntimeState state,
            List<CombatLogEventDto> logs,
            string type,
            ActorState caster,
            ActorState target,
            Dictionary<string, object?> extra = null)
        {
            logs.Add(new CombatLogEventDto(
                state.NowMs(),
                type,
                caster.ActorId.ToString(),
                target.ActorId.ToString(),
                null,
                null,
                extra));
        }
    }
}
