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
                  DamageEffect effect, List<CombatLogEventDto> logs)
        {
            for (int i = 0; i < effect.Hits; i++)
            {
                int dmg = DamageFormula.ComputeWithCrit(
    caster.AtkEff,
    target.DefEff,
    caster.CritRateEff,
    caster.CritDamageEff,
    out bool isCrit
);

                if (effect.HpRatioFactor.HasValue)
                    dmg += (int)(caster.HpMax * effect.HpRatioFactor.Value);

                target.Hp = Math.Max(0, target.Hp - dmg);

                logs.Add(new CombatLogEventDto(
      s.NowMs(),
      "damage",
      caster.ActorId.ToString(),
      target.ActorId.ToString(),
      dmg,
      isCrit,
      new Dictionary<string, object?>
      {
          ["hit"] = i + 1,
          ["totalHits"] = effect.Hits
      }
  ));
            }
        }
    }
}
