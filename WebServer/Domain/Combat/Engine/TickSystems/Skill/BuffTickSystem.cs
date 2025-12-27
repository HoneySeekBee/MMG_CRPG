using Domain.Combat.Runtime;
using Domain.Enum;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Combat.Engine.TickSystems.Skill
{
    public class BuffTickSystem
    {
        public void Run(CombatRuntimeState state, List<CombatLogEvent> logs, int dtMs)
        {
            foreach (var actor in state.ActiveActors.Values)
            {
                if (actor.Dead)
                    continue;

                ProcessBuffs(state, actor, logs, dtMs);
            }
        }
        private void ProcessBuffs(CombatRuntimeState state, ActorState actor, List<CombatLogEvent> logs, int dtMs)
        {
            var expired = new List<AppliedBuff>();

            foreach (var buff in actor.Buffs)
            {
                // 1) 지속시간 감소 
                buff.DurationMs -= dtMs;

                // 2) DOT 적용 (Bleed / Burn / Poison)  
                if (buff.IsDebuff)
                {
                    ApplyDotEffect(state, actor, buff, logs, dtMs);
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

                    logs.Add(new CombatLogEvent(
                        state.NowMs,
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

                logs.Add(new CombatLogEvent(
                    state.NowMs,
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
        private void ApplyDotEffect(CombatRuntimeState state, ActorState actor, AppliedBuff buff, List<CombatLogEvent> logs, int dtMs)
        {
            if (buff.Kind == BuffKind.Bleed)
            {
                int dmg = (int)(buff.Value * (dtMs / 1000f));
                if (dmg <= 0) return;

                actor.Hp = Math.Max(0, actor.Hp - dmg);

                logs.Add(new CombatLogEvent(
                    state.NowMs,
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

    }
}
