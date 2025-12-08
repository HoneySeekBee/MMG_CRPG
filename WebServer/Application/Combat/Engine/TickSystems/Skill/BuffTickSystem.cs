using Application.Combat.Runtime;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems.Skill
{
    public class BuffTickSystem
    {
        public void Run(CombatRuntimeState state, List<CombatLogEventDto> logs)
        {
            foreach (var actor in state.ActiveActors.Values)
            {
                if (actor.Dead)
                    continue;

                ProcessBuffs(state, actor, logs);
                ProcessCcDurations(actor);
            }
        }

        private void ProcessBuffs(
            CombatRuntimeState state,
            ActorState actor,
            List<CombatLogEventDto> logs)
        {
            var expired = new List<AppliedBuff>();

            foreach (var buff in actor.Buffs)
            {
                // 1) 지속시간 감소 
                buff.DurationMs -= 100; // tick이 100ms 기준. 맞게 조정

                // 2) DOT 적용 (Bleed / Burn / Poison)  
                if (buff.IsDebuff)
                {
                    ApplyDotEffect(state, actor, buff, logs);
                }

                // 3) 만료 체크 
                if (buff.DurationMs <= 0)
                    expired.Add(buff);
            }

            // 4) 만료된 버프 제거 
            foreach (var b in expired)
            {
                actor.Buffs.Remove(b);

                // 스탯 버프라면 원복
                if (b.Kind != BuffKind.Shield)
                {
                    RemoveStatChange(actor, b.Kind, b.Value);
                    actor.RecalcStats();
                }
                else
                {
                    // 쉴드 만료
                    actor.Shield -= (int)b.Value;
                    actor.Shield = Math.Max(0, actor.Shield);

                    logs.Add(new CombatLogEventDto(
                        state.NowMs(),
                        "shield_expire",
                        actor.ActorId.ToString(),
                        actor.ActorId.ToString(),
                        null,
                        null,
                        new Dictionary<string, object?>
                        {
                            ["shield"] = actor.Shield
                        }
                    ));
                    continue;
                }

                logs.Add(new CombatLogEventDto(
                    state.NowMs(),
                    "buff_expire",
                    actor.ActorId.ToString(),
                    null,
                    null,
                    null,
                    new Dictionary<string, object?>
                    {
                        ["kind"] = b.Kind.ToString()
                    }
                ));
            }
        }

        // 디버프 종료 
        private void ApplyDotEffect(
           CombatRuntimeState state,
           ActorState actor,
           AppliedBuff buff,
           List<CombatLogEventDto> logs)
        {
            if (buff.Kind == BuffKind.Bleed)
            {
                int dmg = (int)buff.Value;

                actor.Hp = System.Math.Max(0, actor.Hp - dmg);

                logs.Add(new CombatLogEventDto(
                    state.NowMs(),
                    "dot_bleed",
                    actor.ActorId.ToString(),
                    actor.ActorId.ToString(),
                    dmg,
                    false,
                    null));
            }
        }
        private void RemoveStatChange(ActorState target, BuffKind kind, float value)
        {
            switch (kind)
            {
                case BuffKind.AtkUp:
                    target.BuffAtk -= (int)value;
                    break;

                case BuffKind.DefUp:
                    target.BuffDef -= (int)value;
                    break;

                case BuffKind.CritRateUp:
                    target.BuffCritRate -= value;
                    break;

                case BuffKind.CritDamageUp:
                    target.BuffCritDamage -= value;
                    break;

                case BuffKind.DamageReduce:
                    target.BuffDamageReduce -= value;
                    break;

                case BuffKind.FinalDamageReduce:
                    target.BuffFinalDamageReduce -= value;
                    break;

                case BuffKind.DefPenFlat:
                    target.BuffDefPenFlat -= (int)value;
                    break;

                case BuffKind.DefPenPercent:
                    target.BuffDefPenPercent -= value;
                    break;
            }
        }
        private void ProcessCcDurations(ActorState actor)
        {
            int tick = 100;

            if (actor.Stunned)
            {
                actor.StunMs -= tick;
                if (actor.StunMs <= 0)
                {
                    actor.Stunned = false;
                    actor.StunMs = 0;
                }
            }

            if (actor.Silenced)
            {
                actor.SilenceMs -= tick;
                if (actor.SilenceMs <= 0)
                {
                    actor.Silenced = false;
                    actor.SilenceMs = 0;
                }
            }

            if (actor.Rooted)
            {
                actor.RootMs -= tick;
                if (actor.RootMs <= 0)
                {
                    actor.Rooted = false;
                    actor.RootMs = 0;
                }
            }

            if (actor.Frozen)
            {
                actor.FreezeMs -= tick;
                if (actor.FreezeMs <= 0)
                {
                    actor.Frozen = false;
                    actor.FreezeMs = 0;
                }
            }

            if (actor.KnockedDown)
            {
                actor.KnockdownMs -= tick;
                if (actor.KnockdownMs <= 0)
                {
                    actor.KnockedDown = false;
                    actor.KnockdownMs = 0;
                }
            }
        }
    }
}
