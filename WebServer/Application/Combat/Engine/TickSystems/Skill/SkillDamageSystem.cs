using Application.Combat.Runtime;
using Domain.Entities.Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems.Skill
{
    public class SkillDamageSystem
    {
        public void Apply(CombatRuntimeState s, ActorState caster, ActorState target,
                  DamageEffect effect, List<CombatLogEventDto> logs, int hitIndex, float extraMultiplier)
        {
            int dmg = DamageFormula.ComputeWithCrit(
    caster.AtkEff,
    target.DefEff,
    caster.CritRateEff,
    caster.CritDamageEff,
    caster.DefPenFlatEff,
    caster.DefPenPercentEff,
    target.DamageReducePercent,
    target.FinalDamageMultiplier,
    out bool isCrit
);

            // HP 비례 계수 적용
            if (effect.HpRatioFactor.HasValue)
                dmg += (int)(caster.HpMax * effect.HpRatioFactor.Value);

            // delayedHit 등의 추가 배수 적용
            dmg = (int)(dmg * extraMultiplier);
            int rawDamage = dmg;
            // 실드가 존재하면 실드 먼저 감소
            if (target.Shield > 0)
            {
                int absorbed = Math.Min(target.Shield, rawDamage);
                target.Shield -= absorbed;
                rawDamage -= absorbed;

                logs.Add(new CombatLogEventDto(
                    s.NowMs(),
                    "shield_absorb",
                    caster.ActorId.ToString(),
                    target.ActorId.ToString(),
                    absorbed,
                    false,
                    new Dictionary<string, object?>
                    {
                        ["shieldRemain"] = target.Shield
                    }
                ));

                // 데미지가 모두 실드에 흡수되면 HP 감소 없음
                if (rawDamage <= 0)
                    return;
            }

            // 남아있는 데미지를 HP에서 차감
            target.Hp = Math.Max(0, target.Hp - rawDamage);

            // 로그
            logs.Add(new CombatLogEventDto(
                s.NowMs(),
                "damage",
                caster.ActorId.ToString(),
                target.ActorId.ToString(),
                dmg,
                isCrit,
                new Dictionary<string, object?>
                {
                    ["hit"] = hitIndex,
                    ["multiplier"] = extraMultiplier
                }));
        }
    }
} 
