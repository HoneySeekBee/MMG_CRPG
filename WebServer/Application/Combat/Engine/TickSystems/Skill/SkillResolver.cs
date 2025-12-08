using Application.Combat.Runtime;
using Domain.Entities.Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems.Skill
{
    // 스킬 실행 총괄 관리 
    public class SkillResolver
    {
        private readonly SkillDamageSystem _damage = new();
        private readonly SkillHealSystem _heal = new();
        private readonly SkillBuffSystem _buff = new();
        private readonly SkillDebuffSystem _debuff = new();
        private readonly SkillPassiveSystem _passive = new();

        public void Execute(
            CombatRuntimeState state,
            ActorState caster,
            ActorState target,
            SkillEffect effect,
            List<CombatLogEventDto> logs)
        {
            if (effect.Damage != null)
                _damage.Apply(state, caster, target, effect.Damage, logs);

            if (effect.Heal != null)
                _heal.Apply(state, caster, target, effect.Heal, logs);

            if (effect.Buff != null)
                _buff.Apply(state, caster, target, effect.Buff, logs);

            if (effect.Debuff != null)
                _debuff.Apply(state, caster, target, effect.Debuff, logs);

            if (effect.Passive != null)
                _passive.Apply(state, caster, target, effect.Passive, logs);
        }
    }
}
