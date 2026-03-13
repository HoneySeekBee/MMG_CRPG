using Domain.Combat.Runtime;
using Domain.Entities.Skill;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Combat.Engine.TickSystems.Skill
{
    public class SkillHealSystem
    {
        public void Apply(
        CombatRuntimeState s,
        ActorState caster,
        ActorState target,
        HealEffect effect,
        List<CombatLogEvent> logs)
        {
            // 기본 회복량
            float raw = caster.AtkEff * effect.Multiplier;

            // Missing HP 기반 회복
            if (effect.PercentMissingHp.HasValue)
            {
                float missing = target.HpMax - target.Hp;
                raw += missing * effect.PercentMissingHp.Value;
            }

            int heal = Math.Max(1, (int)MathF.Round(raw));
            int before = target.Hp;

            target.Hp = Math.Min(target.HpMax, target.Hp + heal);

            logs.Add(new CombatLogEvent(
                s.NowMs,
                "heal",
                caster.ActorId.ToString(),
                target.ActorId.ToString(),
                heal,
                false,
                null
            ));
        }
    }
}
