using Application.Combat.Runtime;
using Domain.Entities.Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems.Skill
{
    public class SkillPassiveSystem
    {
        public void Apply(
        CombatRuntimeState s,
        ActorState caster,
        ActorState target,
        PassiveEffect effect,
        List<CombatLogEventDto> logs)
        {
            if (effect.AtkPerMissingHpPercent.HasValue)
            {
                float missing = caster.HpMax - caster.Hp;
                caster.AtkBase += (int)(missing * (effect.AtkPerMissingHpPercent.Value / 100f));
            }

            if (effect.PerAllyBonus != null)
            {
                int allies = s.ActiveActors.Values.Count(a => a.Team == caster.Team && !a.Dead);
                foreach (var kv in effect.PerAllyBonus)
                {
                    if (kv.Key == "ATK") caster.AtkBase += (int)(kv.Value * allies);
                    if (kv.Key == "DEF") caster.DefBase += (int)(kv.Value * allies);
                    if (kv.Key == "SPD") caster.SpdBase += (int)(kv.Value * allies);
                }
            }
        }
    }
}
