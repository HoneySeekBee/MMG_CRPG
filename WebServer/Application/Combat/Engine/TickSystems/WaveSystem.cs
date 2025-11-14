using Application.Combat.Runtime;
using Domain.Entities.Contents;
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
            var stage = s.MasterPack?.Stage;
            if (stage == null)
            {
                Console.WriteLine("[WaveSystem] StartWave: s.MasterPack.Stage is null");
                return;
            }

            var wave = stage.Waves[s.CurrentWaveIndex];

            foreach (var spawn in wave.Enemies)
            {
                var def = s.MasterPack.Actors[spawn.MonsterId]; // 기존 생성된 CombatActorDef
                var actor = new ActorState
                {
                    ActorId = spawn.MonsterId,
                    Team = 1, // 몬스터 팀
                    Hp = def.MaxHp,
                    Dead = false,
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

            var stage = s.MasterPack?.Stage;
            if (stage == null)
            {
                Console.WriteLine("[WaveSystem] s.MasterPack.Stage is null");
                return;
            }

            int now = NowMs(s);

            // 0) 이미 웨이브 클리어 후, 다음 웨이브 스폰을 기다리는 중이면
            if (s.WaitingNextWave)
            {
                if (!s.NextWaveSpawnMs.HasValue)
                {
                    bool allPlayersAtSpawn = s.ActiveActors.Values
             .Where(a => a.Team == 0 && !a.Dead && a.Hp > 0)
             .All(a => !a.ReturningToSpawn);

                    if (!allPlayersAtSpawn)
                        return; 

                    // 복귀 
                    s.NextWaveSpawnMs = now + 1000;
                    Console.WriteLine("[WaveSystem] All players reached spawn. Next wave in 1s.");
                    return;
                }

                // 2) 도착 후 대기 시간 카운트 중
                if (now < s.NextWaveSpawnMs.Value)
                    return;

                //  다음 웨이브 시작
                s.WaitingNextWave = false;
                s.NextWaveSpawnMs = null;

                int maxWaveIndex = stage.Waves.Max(w => w.Index);
                if (s.CurrentWaveIndex >= maxWaveIndex)
                {
                    // 예외 처리
                    return;
                }

                s.CurrentWaveIndex++;
                SpawnNextWave(s, evs);
                return;
            }

            //  "전투 중" 로직 

            Console.WriteLine(
                $"[WaveSystem.Run] totalWaves={stage.Waves.Count}, currentWave={s.CurrentWaveIndex}"
            );

            bool anyEnemyAlive = s.ActiveActors.Values
                .Any(a => a.Team == 1 && !a.Dead && a.Hp > 0 && a.Waveindex == s.CurrentWaveIndex);

            Console.WriteLine(
                $"[WaveSystem.Run] anyEnemyAlive={anyEnemyAlive}, currentWave={s.CurrentWaveIndex}"
            );

            if (anyEnemyAlive)
                return;

            Console.WriteLine($"[WaveSystem] Wave {s.CurrentWaveIndex} cleared. Resetting players to spawn.");

            //  현재 웨이브 적 전부 죽음
            ResetPlayerPositionsToSpawn(s);

            evs.Add(new CombatLogEventDto(
                TMs: now,
                Type: "wave_cleared",
                Actor: "",
                Target: "",
                Damage: null,
                Crit: null,
                Extra: new Dictionary<string, object?> { ["wave"] = s.CurrentWaveIndex }
            ));
            int maxWaveIdx = stage.Waves.Max(w => w.Index);

            if (s.CurrentWaveIndex >= maxWaveIdx)
            {
                s.BattleEnded = true;

                evs.Add(new CombatLogEventDto(
                    TMs: now,
                    Type: "stage_cleared",
                    Actor: "",
                    Target: "",
                    Damage: null,
                    Crit: null,
                    Extra: null
                ));
                return;
            }
             
            s.WaitingNextWave = true;
            s.NextWaveSpawnMs = null;
        }
        private void SpawnNextWave(CombatRuntimeState s, List<CombatLogEventDto> evs)
        {
            var stage = s.MasterPack?.Stage;
            if (stage == null)
            {
                Console.WriteLine("[WaveSystem] SpawnNextWave: s.MasterPack.Stage is null");
                return;
            }

            // wave.Index == CurrentWaveIndex 인 웨이브를 찾아온다
            var wave = stage.Waves.FirstOrDefault(w => w.Index == s.CurrentWaveIndex);
            if (wave == null)
            {
                Console.WriteLine($"[WaveSystem] SpawnNextWave: wave index {s.CurrentWaveIndex} not found");
                return;
            }

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
                    Dead = false,
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
                    Waveindex = wave.Index   //  여기도 wave.Index
                };

                s.ActiveActors[actorId] = a;

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
        
        private void ResetPlayerPositionsToSpawn(CombatRuntimeState s)
        {
            foreach (var a in s.ActiveActors.Values)
            {
                if (a.Team != 0) continue;      // 플레이어 팀만
                if (a.Dead || a.Hp <= 0) continue;

                a.ReturningToSpawn = true;
                Console.WriteLine($"[WaveSystem] Actor {a.ActorId} set ReturningToSpawn = true (spawn=({a.SpawnX}, {a.SpawnZ}), pos=({a.X}, {a.Z}))");
                a.TargetActorId = null;
                a.AttackCooldownMs = 0;
                a.SkillCooldownMs = 0;
            }
        }
    }
}
