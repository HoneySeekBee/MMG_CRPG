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

                logs.Add(new CombatLogEventDto(
                    TMs: state.NowMs(),
                    Type: "buff_expire",
                    Actor: actor.ActorId.ToString(),
                    Target: null,
                    Damage: null,
                    Crit: null,
                    Extra: new Dictionary<string, object?>
                    {
                        ["kind"] = b.Kind.ToString()
                    }
                ));
            }
        }

        private void ApplyDotEffect(
            CombatRuntimeState state,
            ActorState actor,
            AppliedBuff buff,
            List<CombatLogEventDto> logs)
        {
            switch (buff.Kind)
            {
                case BuffKind.Bleed:
                    int dmg = (int)buff.Value;

                    int before = actor.Hp;
                    actor.Hp = Math.Max(0, actor.Hp - dmg);

                    logs.Add(new CombatLogEventDto(
                        TMs: state.NowMs(),
                        Type: "dot_bleed",
                        Actor: actor.ActorId.ToString(),
                        Target: actor.ActorId.ToString(),
                        Damage: dmg,
                        Crit: null,
                        Extra: null
                    ));

                    // 죽음 판정은 DeathSystem에서 감지하게 둠
                    break;

                case BuffKind.Burn:
                case BuffKind.Poison:
                    // 나중에 필요하면 추가
                    break;

                default:
                    break;
            }
        }
    }
}
