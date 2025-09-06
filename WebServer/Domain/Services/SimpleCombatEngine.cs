using Domain.Entities;
using Domain.Enum;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public sealed class SimpleCombatEngine : ICombatEngine
    {
        private const int TickMs = 100;

        private sealed class RtUnit
        {
            public string Ref = "";     // "ally:1" / "enemy:2"
            public long CharacterId;
            public int Level;
            public int HpMax;
            public int Hp;
            public int Atk;
            public int Def;
            public int Aspd;
            public float CritRate;
            public float CritDmg;
            public int NextAttackAt;
            public int SkillCd; // 남은 쿨다운(ms)
            public bool IsDead => Hp <= 0;
            public bool IsStunned => false; // MVP: 효과 미구현 자리
            public bool IsAlly;
        }

        public CombatEngineResult Simulate(CombatInputSnapshot input, long seed, MasterDataPack master)
        {
            var rng = new Random(SeedToInt(seed));
            var events = new List<CombatLogEvent>();
            int t = 0;

            // 1) 전투 유닛 구성 (아군 + 스테이지 적)
            var allies = input.Party.Select((p, idx) => BuildUnit($"ally:{idx + 1}", true, p.CharacterId, p.Level, master)).ToList();
            var enemies = master.Stage.Enemies.Select((e, idx) => BuildUnit($"enemy:{idx + 1}", false, e.CharacterId, e.Level, master)).ToList();
            var all = allies.Concat(enemies).ToList();

            // 2) 초기 시간 설정
            foreach (var u in all)
            {
                u.NextAttackAt = u.Aspd; // aspd ms 후 첫 공격
                u.SkillCd = 0;
            }

            // 3) 메인 루프
            // 간단 종료 가드: 5분 제한
            int hardLimitMs = 5 * 60 * 1000;

            while (t <= hardLimitMs && allies.Any(a => !a.IsDead) && enemies.Any(e => !e.IsDead))
            {
                t += TickMs;

                // 상태효과 갱신/스킬 쿨다운 감소
                foreach (var u in all.Where(x => !x.IsDead))
                {
                    if (u.SkillCd > 0) u.SkillCd = Math.Max(0, u.SkillCd - TickMs);
                }

                // 자동 기본공격
                foreach (var u in all.Where(x => !x.IsDead && !x.IsStunned))
                {
                    if (t >= u.NextAttackAt)
                    {
                        var target = PickTarget(u.IsAlly ? enemies : allies);
                        if (target is not null)
                        {
                            var dmg = ComputeDamage(u, target, coeff: 1.0f, rng);
                            target.Hp = Math.Max(0, target.Hp - dmg);
                            events.Add(new CombatLogEvent(t, CombatLogTypes.Basic, u.Ref, target.Ref, dmg, /*crit=*/null, null));

                            if (target.IsDead)
                                events.Add(new CombatLogEvent(t, CombatLogTypes.Death, target.Ref, null, null, null, null));
                        }
                        u.NextAttackAt = t + u.Aspd;
                    }
                }

                // 유저 스킬 입력 처리 (시간이 지났고 쿨다운 OK인 것만)
                foreach (var s in input.SkillInputs.Where(si => si.TMs >= (t - TickMs) && si.TMs < t))
                {
                    var caster = all.FirstOrDefault(x => x.Ref == s.CasterRef && !x.IsDead);
                    if (caster is null) continue;
                    if (caster.SkillCd > 0) continue;

                    // 샘플: 단일 대상 스킬, coeff = master.Skills[s.SkillId].Coeff
                    if (!master.Skills.TryGetValue(s.SkillId, out var sk)) continue;

                    var foeList = caster.IsAlly ? enemies : allies;
                    var target = PickTarget(foeList);
                    if (target is null) continue;

                    var dmg = ComputeDamage(caster, target, sk.Coeff, rng);
                    target.Hp = Math.Max(0, target.Hp - dmg);

                    var extra = new Dictionary<string, object?>
                    {
                        ["skillId"] = s.SkillId,
                        ["coeff"] = sk.Coeff
                    };
                    events.Add(new CombatLogEvent(t, CombatLogTypes.Skill, caster.Ref, target.Ref, dmg, null, extra));
                    if (target.IsDead)
                        events.Add(new CombatLogEvent(t, CombatLogTypes.Death, target.Ref, null, null, null, null));

                    caster.SkillCd = sk.CooldownMs;
                }
            }

            // 4) 결과/클리어시간
            var result = enemies.All(e => e.IsDead) && allies.Any(a => !a.IsDead)
                ? CombatResult.Win
                : allies.All(a => a.IsDead) ? CombatResult.Lose : CombatResult.Error;

            var clearMs = t > hardLimitMs ? (int?)null : t;

            return new CombatEngineResult(result, clearMs ?? t, events);
        }

        private static RtUnit? PickTarget(List<RtUnit> candidates)
            => candidates.Where(c => !c.IsDead)
                         .OrderBy(c => c.Hp)       // HP 낮은 대상 우선
                         .ThenBy(c => c.Ref)
                         .FirstOrDefault();

        private static RtUnit BuildUnit(string unitRef, bool isAlly, long characterId, int level, MasterDataPack master)
        {
            var cd = master.Characters[characterId];
            // 샘플 성장: 레벨당 +2% (매우 단순)
            float growth = 1f + 0.02f * Math.Max(0, level - 1);

            return new RtUnit
            {
                Ref = unitRef,
                IsAlly = isAlly,
                CharacterId = characterId,
                Level = level,
                HpMax = (int)Math.Round(cd.BaseHp * growth),
                Hp = (int)Math.Round(cd.BaseHp * growth),
                Atk = (int)Math.Round(cd.BaseAtk * growth),
                Def = (int)Math.Round(cd.BaseDef * growth),
                Aspd = Math.Max(400, (int)Math.Round(cd.BaseAspd / growth)), // 속도 약간 상향
                CritRate = cd.CritRate,
                CritDmg = cd.CritDmg
            };
        }

        private static int ComputeDamage(RtUnit atk, RtUnit def, float coeff, Random rng)
        {
            var raw = atk.Atk * coeff;
            var mitigated = raw * 100f / (100f + def.Def); // 간단 방어 공식
            var isCrit = rng.NextDouble() < atk.CritRate;
            var dmg = mitigated * (isCrit ? (1f + atk.CritDmg) : 1f);
            return Math.Max(1, (int)Math.Round(dmg));
        }

        private static int SeedToInt(long seed)
        {
            unchecked { return (int)(seed ^ (seed >> 32)); }
        }
    }
}
