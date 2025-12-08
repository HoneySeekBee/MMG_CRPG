using Domain.Entities.Skill;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Application.Combat
{
    public static class SkillEffectParser
    {
        public static SkillEffect Parse(Skill s)
        {
            var json = s.BaseInfo;

            var result = new SkillEffect();

            // Damage
            if (json?["formula"] != null)
                result = result with { Damage = ParseDamage(json) };

            // Buff
            if (json?["buff"] != null)
                result = result with { Buff = ParseBuff(json["buff"]) };

            // Debuff
            if (json?["extra"]?["bleed"] != null)
                result = result with { Debuff = ParseDebuff(json["extra"]) };

            // === 여기서 SkillId를 주입 ===
            if (result.Damage != null)
                result = result with { Damage = result.Damage with { SkillId = s.SkillId } };

            if (result.Buff != null)
                result = result with { Buff = result.Buff with { SkillId = s.SkillId } };

            if (result.Debuff != null)
                result = result with { Debuff = result.Debuff with { SkillId = s.SkillId } };
            if (result.Heal != null)
                result = result with { Heal = result.Heal with { SkillId = s.SkillId } };

            if (result.Passive != null)
                result = result with { Passive = result.Passive with { SkillId = s.SkillId } };
            return result;
        }
        private static DamageEffect ParseDamage(JsonNode json)
        {
            string formula = json["formula"]!.GetValue<string>();

            float multiplier = 1f;
            int hits = json["hits"]?.GetValue<int>() ?? 1;

            // 예: "ATK * 1.5"
            if (formula.Contains("*"))
            {
                var parts = formula.Split('*');
                multiplier = float.Parse(parts[1]);
            }

            return new DamageEffect
            {
                Hits = hits,
                Multiplier = multiplier
            };
        }
        private static BuffEffect ParseBuff(JsonNode buff)
        {
            float value = 0f;
            int duration = buff["duration"]?.GetValue<int>() ?? 1000;

            if (buff["atkUp"] != null)
                value = buff["atkUp"]!.GetValue<float>();

            return new BuffEffect
            {
                SkillId = 0, // SkillId는 SkillEffectParser.Parse()에서 넣어라
                Kind = BuffKind.AtkUp,
                Value = value,
                DurationMs = duration * 1000,
                MaxStacks = 1
            };
        }
        private static DebuffEffect ParseDebuff(JsonNode extra)
        {
            var bleed = extra["bleed"]!;

            return new DebuffEffect
            {
                SkillId = 0,  
                BleedDamage = bleed["damage"]?.GetValue<int>(),
                BleedDuration = bleed["duration"]?.GetValue<int>()
            };
        }
        private static HealEffect ParseHeal(JsonNode healJson)
        {
            return new HealEffect
            {
                SkillId = 0,  
                Multiplier = healJson["multiplier"]?.GetValue<float>() ?? 1f,
                PercentMissingHp = healJson["missingPercent"]?.GetValue<float>()
            };
        }
        private static PassiveEffect ParsePassive(JsonNode p)
        {
            return new PassiveEffect
            {
                SkillId = 0,
                AtkPerMissingHpPercent = p["atkPerMissingHpPercent"]?.GetValue<float>(),
                HealPercent = p["healPercent"]?.GetValue<float>(),
                CooldownReduceSec = p["cooldownReduceSec"]?.GetValue<float>(),
                ShieldPerAlly = p["shieldPerAlly"]?.GetValue<float>(),
                SelfHpCostPercent = p["selfHpCostPercent"]?.GetValue<float>(),
                DefPerUse = p["defPerUse"]?.GetValue<int>(),
                MaxStacks = p["maxStacks"]?.GetValue<int>(),
            };
        }
    }
}
