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
        private readonly Random _rng = new(); // TODO: 나중에 Seed/IRandomProvider로 교체

        public void Run(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            foreach (var actor in s.Snapshot.Actors.Values.Where(a => !a.Dead))
            {
                if (actor.AttackCooldownMs > 0)
                {
                    actor.AttackCooldownMs = Math.Max(0, actor.AttackCooldownMs - TickMs);
                    continue;
                }
                if (actor.TargetActorId != null)
                {
                    var t = s.Snapshot.Actors[actor.TargetActorId.Value];
                    if (t.Dead)
                    {
                        actor.TargetActorId = null;
                    }
                }

                if (actor.TargetActorId == null)
                    actor.TargetActorId = FindNearestEnemy(s, actor.ActorId);
                if (actor.TargetActorId == null)
                    continue;

                var target = s.Snapshot.Actors[actor.TargetActorId.Value];
                float dist = Distance(actor, target);

                if (dist > actor.Range) //  각자 사거리
                    continue;

                // === 데미지 ===
                int baseDmg = Math.Max(1, actor.Atk - target.Def);

                bool isCrit = _rng.NextDouble() < actor.CritRate;
                int finalDmg = isCrit
                    ? (int)MathF.Round(baseDmg * (1f + (float)actor.CritDamage))
                    : baseDmg;

                target.Hp -= finalDmg;
                actor.AttackCooldownMs = actor.AttackIntervalMs; //  각자 공격속도

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
            if (!s.Snapshot.Actors.TryGetValue(actorId, out var self))
                return null;

            float nearestDist = float.MaxValue;
            long? nearestId = null;

            foreach (var other in s.Snapshot.Actors.Values)
            {
                // 같은 팀은 제외
                if (other.Team == self.Team) continue;

                // 죽은 적은 제외
                if (other.Dead) continue;

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
