using Domain.Combat.Runtime;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Combat.Engine.TickSystems
{
    public sealed class AttackSystem
    {
        private const float AttackSpeedScale = 2.0f;
        public void Run(CombatRuntimeState s, List<CombatLogEvent> evs, int dtMs)
        {
            foreach (var actor in s.ActiveActors.Values.Where(a => !a.Dead && a.Hp > 0))
            {
                // 쿨타임 감소
                if (actor.AttackCooldownMs > 0)
                {
                    actor.AttackCooldownMs = Math.Max(0, actor.AttackCooldownMs - dtMs);
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
                    continue;

                var target = s.ActiveActors[actor.TargetActorId.Value];

                // 타겟이 이미 죽어있으면 스킵
                if (target.Dead || target.Hp <= 0) // 방어 한 번 더
                {
                    actor.TargetActorId = null;
                    continue;
                }

                float dist = Distance(actor, target);
                float effectiveRange = actor.RangeBase;

                if (dist > effectiveRange)
                    continue;
                int baseDmg = DamageFormula.ComputeBase(actor.AtkEff, target.DefEff);
                int finalDmg = DamageFormula.ComputeWithCrit(
                    actor.AtkEff,
                    target.DefEff,
                    actor.CritRateEff,
                    actor.CritDamageEff,
                    actor.DefPenFlat,
                    actor.DefPenPercent,
                    target.DamageReducePercent,
                    target.FinalDamageMultiplier,
                    out bool isCrit
                );

                int oldHp = target.Hp;
                // HP 깎고 0으로 클램프만, Dead 플래그는 DeathSystem에서
                target.Hp -= finalDmg;
                if (target.Hp < 0)
                    target.Hp = 0;

                actor.AttackCooldownMs = (int)(actor.AttackIntervalMsBase / AttackSpeedScale);

                evs.Add(new CombatLogEvent(
                  s.NowMs,
                   "normal_attack",
                   actor.ActorId.ToString(),
                   target.ActorId.ToString(),
                   finalDmg,
                   isCrit,
                   new Dictionary<string, object?> { ["isNormal"] = true }
                 ));
            }
        }

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
    }
}
