using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public sealed class AttackSystem
    {
        private const int TickMs = 100;      // 1틱 = 100ms
        private const float AttackSpeedScale = 2.0f;
        private readonly Random _rng = new(); // TODO: 나중에 Seed/IRandomProvider로 교체
        const float PaddingDist = 1.0f;
        public void Run(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            foreach (var actor in s.ActiveActors.Values.Where(a => !a.Dead && a.Hp > 0)) // Hp>0 추가
            {
                Console.WriteLine($"[Atk] Actor={actor.ActorId}, Team={actor.Team}, Hp={actor.Hp}, Cd={actor.AttackCooldownMs}");

                // 쿨타임 감소
                if (actor.AttackCooldownMs > 0)
                {
                    actor.AttackCooldownMs = Math.Max(0, actor.AttackCooldownMs - TickMs);
                    continue;
                }

                // 기존 타겟이 죽었거나 없으면 타겟 초기화
                if (actor.TargetActorId != null)
                {
                    if (!s.ActiveActors.TryGetValue(actor.TargetActorId.Value, out var t) || t.Dead || t.Hp <= 0) // Hp<=0 체크
                        actor.TargetActorId = null;
                }

                // 타겟 없으면 새로 찾기
                if (actor.TargetActorId == null)
                    actor.TargetActorId = FindNearestEnemy(s, actor.ActorId);

                if (actor.TargetActorId == null)
                {
                    Console.WriteLine($"[Atk] Actor={actor.ActorId} -> target 없음");
                    continue;
                }

                var target = s.ActiveActors[actor.TargetActorId.Value];

                // 타겟이 이미 죽어있으면 스킵
                if (target.Dead || target.Hp <= 0) // 방어 한 번 더
                {
                    actor.TargetActorId = null;
                    continue;
                }

                float dist = Distance(actor, target);
                float effectiveRange = actor.Range + PaddingDist;
                Console.WriteLine($"[Atk] {actor.ActorId}(Team={actor.Team}) -> {target.ActorId}(Team={target.Team}) dist={dist}, range={actor.Range}");

                if (dist > effectiveRange)
                    continue;

                int baseDmg = ComputeBaseDamage(actor.Atk, target.Def);

                bool isCrit = _rng.NextDouble() < actor.CritRate;
                int finalDmg = isCrit
                    ? (int)MathF.Round(baseDmg * (1f + (float)actor.CritDamage))
                    : baseDmg;

                int oldHp = target.Hp;
                // HP 깎고 0으로 클램프만, Dead 플래그는 DeathSystem에서
                target.Hp -= finalDmg;
                if (target.Hp < 0)
                    target.Hp = 0;

                actor.AttackCooldownMs = (int)(actor.AttackIntervalMs * AttackSpeedScale);

                Console.WriteLine(
                    $"[Hit] {actor.ActorId}(T={actor.Team}) -> {target.ActorId}(T={target.Team}), " +
                    $"dmg={finalDmg}, hp: {oldHp} -> {target.Hp}"
                );
                evs.Add(new CombatLogEventDto(
                    TMs: NowMs(s),
                    Type: "hit",
                    Actor: actor.ActorId.ToString(),
                    Target: target.ActorId.ToString(),
                    Damage: finalDmg,
                    Crit: isCrit,
                    Extra: null
                ));
            }
        }
        private int NowMs(CombatRuntimeState s)
            => (int)(DateTimeOffset.UtcNow - s.StartedAt).TotalMilliseconds;

        private float Distance(ActorState a, ActorState b)
        {
            float dx = a.X - b.X;
            float dz = a.Z - b.Z;
            return MathF.Sqrt(dx * dx + dz * dz);
        }

        private long? FindNearestEnemy(CombatRuntimeState s, long actorId)
        {
            if (!s.ActiveActors.TryGetValue(actorId, out var self))
                return null;

            float nearestDist = float.MaxValue;
            long? nearestId = null;

            foreach (var other in s.ActiveActors.Values)
            {
                if (other.Team == self.Team) continue;
                if (other.Dead || other.Hp <= 0) continue; // Hp<=0 추가

                float dx = other.X - self.X;
                float dz = other.Z - self.Z;
                float dist = MathF.Sqrt(dx * dx + dz * dz);

                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestId = other.ActorId;
                }
            }

            return nearestId;
        }
        private int ComputeBaseDamage(int atk, int def)
        {
            // float/MathF 써서 실수 연산
            float fAtk = atk;
            float fDef = Math.Max(def, 0);  

            // 1. 고정 방어량 (DEF의 20%)
            float guaranteedBlock = fDef / 5f;          // DEF * 0.2f

            // 2. 남은 80% 방어력
            float remainingDef = fDef * 4f / 5f;        // DEF * 0.8f

            // 3. 추가 방어 비율 = DEF / 100, 단 최대 0.9 (= 90%)
            float rateRaw = fDef / 100f;
            float rate = MathF.Min(rateRaw, 0.9f);

            // 4. 추가 방어량
            float extraBlock = remainingDef * rate;

            // 5. 총 방어량
            float totalBlock = guaranteedBlock + extraBlock;

            // 6. 실제 데미지
            float rawDamage = fAtk - totalBlock;

            // 최소 1 보장 + 반올림
            int baseDamage = Math.Max(1, (int)MathF.Round(rawDamage));

            return baseDamage;
        }
    }
}
