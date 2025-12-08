using Application.Combat.Runtime;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems.Skill
{
    public class BuffStatSystem
    {
        public void Run(CombatRuntimeState state)
        {
            foreach (var actor in state.ActiveActors.Values)
            {
                if (!actor.Dead)
                    Recalculate(actor);
            }
        }
        private void Recalculate(ActorState actor)
        {
            // 1) 모든 버프 효과 값 초기화
            actor.BuffAtk = 0;
            actor.BuffDef = 0;
            actor.BuffCritRate = 0;
            actor.BuffCritDamage = 0;
            actor.SpdEff = actor.SpdBase;  // 속도는 Eff 값 직접 보유
            actor.RangeEff = actor.RangeBase;

            actor.BuffDamageReduce = 0f;
            actor.BuffFinalDamageReduce = 0f;
            actor.BuffDefPenFlat = 0;
            actor.BuffDefPenPercent = 0f;

            // 2) 버프 재계산
            foreach (var buff in actor.Buffs)
            {
                float value = buff.Value * buff.Stacks;

                switch (buff.Kind)
                {
                    case BuffKind.AtkUp:
                        actor.BuffAtk += (int)(actor.AtkBase * value);
                        break;

                    case BuffKind.DefUp:
                        actor.BuffDef += (int)(actor.DefBase * value);
                        break;

                    case BuffKind.SpdUp:
                        actor.SpdEff += (int)(actor.SpdBase * value);
                        break;

                    case BuffKind.CritRateUp:
                        actor.BuffCritRate += value;
                        break;

                    case BuffKind.CritDamageUp:
                        actor.BuffCritDamage += value;
                        break;

                    case BuffKind.AtkDown:
                        actor.BuffAtk -= (int)(actor.AtkBase * value);
                        break;

                    case BuffKind.DefDown:
                        actor.BuffDef -= (int)(actor.DefBase * value);
                        break;

                    case BuffKind.DamageReduce:
                        actor.BuffDamageReduce += value;
                        break;

                    case BuffKind.FinalDamageReduce:
                        actor.BuffFinalDamageReduce += value;
                        break;

                    case BuffKind.DefPenFlat:
                        actor.BuffDefPenFlat += (int)value;
                        break;

                    case BuffKind.DefPenPercent:
                        actor.BuffDefPenPercent += value;
                        break;

                    // DOT, CC는 다른 시스템에서 처리
                    case BuffKind.Burn:
                    case BuffKind.Bleed:
                    case BuffKind.Poison:
                    case BuffKind.Stun:
                    case BuffKind.Silence:
                        break;
                }
            }

            // 3) 클램핑
            actor.SpdEff = Math.Max(1, actor.SpdEff);
            actor.BuffCritRate = Math.Clamp(actor.BuffCritRate, 0, 1);
        }
    }
}
