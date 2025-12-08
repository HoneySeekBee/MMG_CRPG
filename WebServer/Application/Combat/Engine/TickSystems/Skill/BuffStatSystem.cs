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
            // 1) 기본값 초기화 (Base → Eff) 
            actor.AtkEff = actor.AtkBase;
            actor.DefEff = actor.DefBase;
            actor.SpdEff = actor.SpdBase;
            actor.CritRateEff = actor.CritRateBase;
            actor.CritDamageEff = actor.CritDamageBase;
            actor.RangeEff = actor.RangeBase;
             
            // 2) 버프 계산 
            foreach (var buff in actor.Buffs)
            {
                float value = buff.Value * buff.Stacks;

                switch (buff.Kind)
                {
                    // ===== Buffs ===== //
                    case BuffKind.AtkUp:
                        actor.AtkEff = (int)(actor.AtkEff * (1 + value));
                        break;

                    case BuffKind.DefUp:
                        actor.DefEff = (int)(actor.DefEff * (1 + value));
                        break;

                    case BuffKind.SpdUp:
                        actor.SpdEff = (int)(actor.SpdEff * (1 + value));
                        break;

                    case BuffKind.CritUp:
                        actor.CritRateEff += value;
                        break;

                    // ===== Debuffs ===== //
                    case BuffKind.AtkDown:
                        actor.AtkEff = (int)(actor.AtkEff * (1 - value));
                        break;

                    case BuffKind.DefDown:
                        actor.DefEff = (int)(actor.DefEff * (1 - value));
                        break;

                    case BuffKind.Burn:
                    case BuffKind.Bleed:
                    case BuffKind.Poison:
                    case BuffKind.Stun:
                    case BuffKind.Silence:
                        // DOT / CC는 별 시스템에서 처리하므로 여기선 스탯 변경 없음
                        break;
                }
            }
             
            // 3) 클램핑 처리 (오류 방지) 
            actor.AtkEff = Math.Max(1, actor.AtkEff);
            actor.DefEff = Math.Max(0, actor.DefEff);
            actor.SpdEff = Math.Max(1, actor.SpdEff);
            actor.CritRateEff = Math.Clamp(actor.CritRateEff, 0, 1);
            actor.CritDamageEff = Math.Max(0, actor.CritDamageEff);
        }
    }
}
