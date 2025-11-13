using Application.Combat.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Engine.TickSystems
{
    public sealed class WaveSystem
    {
        public void StartWave(CombatRuntimeState s)
        {
            var wave = s.StageDef.Waves[s.CurrentWaveIndex];

            foreach (var spawn in wave.Enemies)
            {
                var def = s.MasterPack.Actors[spawn.MonsterId]; // 기존 생성된 CombatActorDef
                var actor = new ActorState
                {
                    ActorId = spawn.MonsterId,
                    Team = 1, // 몬스터 팀
                    Hp = def.MaxHp,
                    Atk = def.Atk,
                    Def = def.Def,
                    Spd = def.Spd,
                    Range = def.Range,
                    AttackIntervalMs = def.AttackIntervalMs,
                    CritRate = def.CritRate,
                    CritDamage = def.CritDamage,
                    TargetActorId = null,
                    Waveindex = s.CurrentWaveIndex
                };

                // 초기 위치 잡기 (기본 X,Z 같은 것)
                var (x, z) = PositionUtils.GetEnemyPositionBySlot(spawn.Slot);

                actor.X = x;
                actor.Z = z;
                s.ActiveActors[actor.ActorId] = actor;
            }
        }
        public void CheckWaveTransition(CombatRuntimeState s)
        {
            bool allEnemiesDead = s.ActiveActors.Values
                .Where(a => a.Team == 1)
                .All(a => a.Dead);

            if (!allEnemiesDead)
                return;

            // 다음 웨이브 존재?
            if (s.CurrentWaveIndex + 1 < s.StageDef.Waves.Count)
            {
                s.CurrentWaveIndex++;
                StartWave(s);
            }
            else
            {
                s.BattleEnded = true; // 전투 종료
            }
        }

        public void Run(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            if (s.BattleEnded)
                return;

            // 1) 현재 웨이브의 적이 살아있는지 확인
            bool anyEnemyAlive = s.Snapshot.Actors.Values
                .Any(a => a.Team == 1 && !a.Dead && a.Waveindex == s.CurrentWaveIndex);

            if (anyEnemyAlive)
                return;

            // 2) wave cleared 이벤트
            evs.Add(new CombatLogEventDto(
                TMs: NowMs(s),
                Type: "wave_cleared",
                Actor: "",
                Target: "",
                Damage: null,
                Crit: null,
                Extra: new Dictionary<string, object?> { ["wave"] = s.CurrentWaveIndex }
            ));

            // 3) 마지막 웨이브면 종료
            if (s.CurrentWaveIndex >= s.StageDef.Waves.Count - 1)
            {
                s.BattleEnded = true;

                evs.Add(new CombatLogEventDto(
                    TMs: NowMs(s),
                    Type: "stage_cleared",
                    Actor: "",
                    Target: "",
                    Damage: null,
                    Crit: null,
                    Extra: null
                ));

                return;
            }

            // 4) 다음 웨이브 시작
            s.CurrentWaveIndex++;
            SpawnNextWave(s, evs);
        }

        private void SpawnNextWave(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            var wave = s.StageDef.Waves[s.CurrentWaveIndex];

            foreach (var spawn in wave.Enemies)
            {
                var def = s.MasterPack.Actors[spawn.MonsterId];
                var (x, z) = PositionUtils.GetEnemyPositionBySlot(spawn.Slot);
                var actorId = 1000 * wave.Index + spawn.Slot;

                var a = new ActorState
                {
                    ActorId = actorId,
                    Team = 1,
                    X = x,
                    Z = z,
                    Hp = def.MaxHp,
                    Atk = def.Atk,
                    Def = def.Def,
                    Spd = def.Spd,
                    Range = def.Range,
                    AttackIntervalMs = def.AttackIntervalMs,
                    CritRate = def.CritRate,
                    CritDamage = def.CritDamage,
                    AttackCooldownMs = 0,
                    SkillCooldownMs = 0,
                    TargetActorId = null,
                    Waveindex = wave.Index
                };

                s.Snapshot.Actors[actorId] = a;

                evs.Add(new CombatLogEventDto(
                    TMs: NowMs(s),
                    Type: "spawn",
                    Actor: actorId.ToString(),
                    Target: null,
                    Damage: null,
                    Crit: null,
                    Extra: new Dictionary<string, object?> { ["wave"] = wave.Index }
                ));
            }
        }
        private int NowMs(CombatRuntimeState s)
            => (int)(DateTimeOffset.UtcNow - s.StartedAt).TotalMilliseconds;
    }
}
