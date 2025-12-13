using Domain.Entities.Skill;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Combat
{
    public static class SkillEffectParser
    {
        public static SkillEffect SafeParseEffect(int skillId, JsonNode? values)
        {
            try { return Parse(skillId, values); }
            catch (Exception ex)
            {
                Console.WriteLine($"SkillEffect parse fail skillId={skillId} err={ex.Message}");
                return new SkillEffect();
            }
        }
        public static SkillEffect Parse(Skill s)
        { 
            return Parse(s.SkillId, s.BaseInfo);
        }

        public static SkillEffect Parse(int skillId, JsonNode? json)
        {
            if (json is null) return new SkillEffect();
            var result = new SkillEffect();
            if (json is null) return result;

            if (IsHeal(json))
                result = result with { Heal = ParseHeal(json) };

            var dmg = TryParseDamage(json);
            if (dmg != null)
                result = result with { Damage = dmg };

            if (json["buff"] != null)
                result = result with { Buff = ParseBuff(json["buff"]!) };

            if (json["bleed"] != null)
                result = result with { Debuff = ParseBleed(json["bleed"]!) };

            if (json["extra"]?["delayedHit"] != null)
                result = result with { Debuff = MergeDebuff(result.Debuff, ParseDelayedHit(json["extra"]!["delayedHit"]!)) };

            if (json["passive"] is JsonObject pobj)
                result = result with { Passive = ParsePassive(pobj) };
            if (result.Damage != null) result = result with { Damage = result.Damage with { SkillId = skillId } };
            if (result.Heal != null) result = result with { Heal = result.Heal with { SkillId = skillId } };
            if (result.Buff != null) result = result with { Buff = result.Buff with { SkillId = skillId } };
            if (result.Debuff != null) result = result with { Debuff = result.Debuff with { SkillId = skillId } };
            if (result.Passive != null) result = result with { Passive = result.Passive with { SkillId = skillId } };

            return result;
        }
        private static bool IsHeal(JsonNode json)
        {
            if (json["heal"]?.GetValue<bool?>() == true) return true;
            var f = json["formula"]?.GetValue<string?>();
            if (string.Equals(f, "HEAL", StringComparison.OrdinalIgnoreCase)) return true;
            if (json["healMultiplier"] != null) return true;
            if (json["healPercent"] != null) return true;
            return false;
        }
        private static HealEffect ParseHeal(JsonNode json)
        {
            // Values: healMultiplier, healPercent
            var mult = GetFloat(json["healMultiplier"]) ?? 1f;
            var missing = GetFloat(json["missingPercent"]);  
            return new HealEffect
            {
                SkillId = 0,
                Multiplier = mult,
                PercentMissingHp = missing
            };
        }

        private static DamageEffect? TryParseDamage(JsonNode json)
        {
            // heal 토큰이면 damage 만들지 않음
            var f = json["formula"]?.GetValue<string?>();
            if (string.Equals(f, "HEAL", StringComparison.OrdinalIgnoreCase))
                return null;

            int hits = json["hits"]?.GetValue<int>() ?? 1;

            // 1) direct multiplier 형태
            var direct = GetFloat(json["damageMultiplier"]) ?? GetFloat(json["multiplier"]);
            if (direct != null)
            {
                var dmg0 = new DamageEffect
                {
                    SkillId = 0,
                    Hits = hits,
                    AtkMultiplier = direct.Value
                };
                return ApplyDamageExtras(json, dmg0);
            }

            // 2) formula 형태
            if (json["formula"] == null) return null;

            var formula = json["formula"]!.GetValue<string>().Trim();

            bool usesRange = formula.Contains("range", StringComparison.OrdinalIgnoreCase);
            float? minR = GetFloat(json["range"]?["min"]);
            float? maxR = GetFloat(json["range"]?["max"]);

            var dmg = new DamageEffect
            {
                SkillId = 0,
                Hits = hits,
                UsesRange = usesRange,
                MinRange = usesRange ? minR : null,
                MaxRange = usesRange ? maxR : null,

                // ATK/HP/DEF 항 추출
                AtkMultiplier = ExtractMul(formula, "ATK") ?? (ContainsBareStat(formula, "ATK") ? 1f : 0f),
                HpMultiplier = ExtractMul(formula, "HP") ?? (ContainsBareStat(formula, "HP") ? 1f : 0f),
                DefMultiplier = ExtractMul(formula, "DEF") ?? (ContainsBareStat(formula, "DEF") ? 1f : 0f),
                Flat = ExtractFlat(formula)
            };

            // "ATK * range" 케이스: ATK 계수 없으면 1 취급
            if (dmg.UsesRange && dmg.AtkMultiplier == 0f && formula.Contains("ATK", StringComparison.OrdinalIgnoreCase))
                dmg = dmg with { AtkMultiplier = 1f };

            return ApplyDamageExtras(json, dmg);
        }
        private static DamageEffect ApplyDamageExtras(JsonNode json, DamageEffect dmg)
        {
            if (json["isAoe"]?.GetValue<bool?>() == true)
                dmg = dmg with { IsAoe = true };

            var targetLimit = json["targetLimit"]?.GetValue<int?>();
            if (targetLimit != null)
                dmg = dmg with { TargetLimit = targetLimit };

            if (json["extra"]?["pathDamage"]?.GetValue<bool?>() == true)
                dmg = dmg with { PathDamage = true };

            var aoeRange = json["extra"]?["aoeRange"]?.GetValue<int?>();
            if (aoeRange != null)
                dmg = dmg with { AoeRange = aoeRange, IsAoe = true };

            return dmg;
        }
        private static float? ExtractMul(string formula, string stat)
        {
            // ex) "HP * 0.2"
            var m = Regex.Match(formula, $@"\b{stat}\b\s*\*\s*([0-9]+(?:\.[0-9]+)?)", RegexOptions.IgnoreCase);
            if (!m.Success) return null;
            return float.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        }
        private static bool ContainsBareStat(string formula, string stat)
        {
            if (!Regex.IsMatch(formula, $@"\b{stat}\b", RegexOptions.IgnoreCase)) return false;
            if (Regex.IsMatch(formula, $@"\b{stat}\b\s*\*", RegexOptions.IgnoreCase)) return false;
            return true;
        }
        private static float ExtractFlat(string formula)
        {
            float sum = 0f;
            foreach (Match m in Regex.Matches(formula, @"([+\-])\s*([0-9]+(?:\.[0-9]+)?)"))
            {
                // 계수("* 1.5")는 제외하려고 바로 앞이 '*'인지 체크
                var idx = m.Index;
                if (idx > 0 && formula[..idx].TrimEnd().EndsWith("*")) continue;

                var sign = m.Groups[1].Value == "-" ? -1f : 1f;
                var val = float.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                sum += sign * val;
            }
            return sum;
        }
        private static BuffEffect ParseBuff(JsonNode buff)
        {
            int durationSec = buff["duration"]?.GetValue<int>() ?? 1;

            if (buff["atkUp"] != null)
            {
                return new BuffEffect
                {
                    SkillId = 0,
                    Kind = BuffKind.AtkUp,
                    Value = buff["atkUp"]!.GetValue<float>(),
                    DurationMs = durationSec * 1000,
                    MaxStacks = 1
                };
            }

            if (buff["defBonus"] != null)
            {
                return new BuffEffect
                {
                    SkillId = 0,
                    Kind = BuffKind.DefUp,
                    Value = buff["defBonus"]!.GetValue<float>(),
                    DurationMs = durationSec * 1000,
                    MaxStacks = 1
                };
            }

            if (buff["resetSkillCooldown"]?.GetValue<bool?>() == true)
            { 
                Console.WriteLine("resetSkillCooldown buff exists but not modeled.");
            }

            return new BuffEffect
            {
                SkillId = 0,
                Kind = BuffKind.AtkUp,
                Value = 0f,
                DurationMs = durationSec * 1000,
                MaxStacks = 1
            };
        }

        private static DebuffEffect ParseBleed(JsonNode bleed)
        {
            return new DebuffEffect
            {
                SkillId = 0,
                BleedDamage = bleed["damage"]?.GetValue<int>(),
                BleedDuration = bleed["duration"]?.GetValue<int>()
            };
        }
        private static DebuffEffect ParseDelayedHit(JsonNode delayed)
        {
            return new DebuffEffect
            {
                SkillId = 0,
                DelayedDelaySec = GetFloat(delayed["delay"]),
                DelayedMultiplier = GetFloat(delayed["multiplier"])
            };
        }

        private static DebuffEffect MergeDebuff(DebuffEffect? a, DebuffEffect b)
        {
            if (a == null) return b;
            return a with
            {
                BleedDamage = a.BleedDamage ?? b.BleedDamage,
                BleedDuration = a.BleedDuration ?? b.BleedDuration,
                DelayedDelaySec = a.DelayedDelaySec ?? b.DelayedDelaySec,
                DelayedMultiplier = a.DelayedMultiplier ?? b.DelayedMultiplier,
            };
        }
        private static PassiveEffect ParsePassive(JsonNode p)
        {
            return new PassiveEffect
            {
                SkillId = 0,
                AtkPerMissingHpPercent = p["atkPerMissingHpPercent"]?.GetValue<float?>(),
                HealPercent = p["healPercent"]?.GetValue<float?>(),
                CooldownReduceSec = p["cooldownReduceSec"]?.GetValue<float?>(),
                ShieldPerAlly = p["shieldPerAlly"]?.GetValue<float?>(),
                SelfHpCostPercent = p["selfHpCostPercent"]?.GetValue<float?>(),
                DefPerUse = p["defPerUse"]?.GetValue<int?>(),
                MaxStacks = p["maxStacks"]?.GetValue<int?>(),
            };
        }

        private static float? GetFloat(JsonNode? node)
        {
            if (node is null) return null;
            try
            {
                if (node is JsonValue v)
                {
                    if (v.TryGetValue<float>(out var f)) return f;
                    if (v.TryGetValue<double>(out var d)) return (float)d;
                    if (v.TryGetValue<int>(out var i)) return i;
                    if (v.TryGetValue<string>(out var s) &&
                        float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                        return x;
                }
            }
            catch { }
            return null;
        }
    }
}
