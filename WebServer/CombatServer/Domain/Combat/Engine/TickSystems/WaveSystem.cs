using Domain.Combat.Runtime;
using Domain.Enum;
using Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Combat.Engine.TickSystems
{
    public sealed class WaveSystem
    {
        public void Run(CombatRuntimeState s, List<CombatLogEvent> evs)
        {
            if (s.BattleEnded)
                return;

            var stage = s._MasterPack?.Stage;
            if (stage == null)
                return;
            int now = s.NowMs;

            // 이미 웨이브 클리어 후, 다음 웨이브 스폰을 기다리는 중이면
            if (s.WaitingNextWave)
            {
                if (!s.NextWaveSpawnMs.HasValue)
                {
                    var players = s.ActiveActors.Values
        .Where(a => a.Team == 0 && !a.Dead && a.Hp > 0)
        .ToList();
                    bool allPlayersAtSpawn = players.All(a => a.ArrivedAtSpawn || IsAtSpawn(a));

                    if (!allPlayersAtSpawn)
                        return;

                    foreach (var p in players)
                    {
                        p.ReturningToSpawn = false;
                    }

                    s.NextWaveSpawnMs = now + 1000;
                    return;
                }

                // 2) 도착 후 대기 시간 카운트 중
                if (now < s.NextWaveSpawnMs.Value)
                    return;

                //  다음 웨이브 시작
                s.WaitingNextWave = false;
                s.NextWaveSpawnMs = null;

                int maxWaveIndex = stage.Waves.Max(w => w.Index);
                if (s.WaitingStageResult && s.CurrentWaveIndex >= maxWaveIndex)
                {
                    s.WaitingStageResult = false;

                    s.Result = s.PendingStageResult == "lose" ? CombatResult.Lose : CombatResult.Win;
                    s.Phase = CombatBattlePhase.Completed;
                    s.BattleEnded = true;

                    evs.Add(new CombatLogEvent(
                        TMs: now,
                        Type: "stage_result",
                        Actor: "",
                        Target: "",
                        Damage: null,
                        Crit: null,
                        Extra: new Dictionary<string, object?> { ["result"] = s.PendingStageResult ?? "win" }
                    ));

                    return;
                }

                s.CurrentWaveIndex++;
                SpawnNextWave(s, evs);
                return;
            }

            bool anyEnemyAlive = s.ActiveActors.Values
                .Any(a => a.Team == 1 && !a.Dead && a.Hp > 0 && a.Waveindex == s.CurrentWaveIndex);

            if (anyEnemyAlive)
                return;

            //  현재 웨이브 적 전부 죽음
            CleanupWaveEnemies(s);
            ResetPlayerPositionsToSpawn(s);

            evs.Add(new CombatLogEvent(
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
                s.WaitingStageResult = true;
                s.PendingStageResult = "win";

                s.WaitingNextWave = true;
                s.NextWaveSpawnMs = null;
                return;
            }

            s.WaitingNextWave = true;
            s.NextWaveSpawnMs = null;
        }
        private void SpawnNextWave(CombatRuntimeState s, List<CombatLogEvent> evs)
        {
            var stage = s._MasterPack?.Stage;
            if (stage == null)
                return;

            var wave = stage.Waves.FirstOrDefault(w => w.Index == s.CurrentWaveIndex);
            if (wave == null)
                return;

            foreach (var spawn in wave.Enemies)
            {
                var def = s._MasterPack.Actors[spawn.MonsterId];
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
                    AtkBase = def.Atk,
                    DefBase = def.Def,
                    SpdBase = def.Spd,
                    RangeBase = def.Range,
                    AttackIntervalMsBase = def.AttackIntervalMs,
                    CritRateBase = def.CritRate,
                    CritDamageBase = def.CritDamage,
                    AttackCooldownMs = 0,
                    SkillCooldownMs = 0,
                    TargetActorId = null,
                    Waveindex = wave.Index   //  여기도 wave.Index
                };

                s.ActiveActors[actorId] = a;

                evs.Add(new CombatLogEvent(
                    TMs: s.NowMs,
                    Type: "spawn",
                    Actor: actorId.ToString(),
                    Target: null,
                    Damage: null,
                    Crit: null,
                    Extra: new Dictionary<string, object?> { ["wave"] = wave.Index }
                ));
            }
        }
        private bool IsAtSpawn(ActorState a, float radius = 0.25f)
        {
            var dx = a.X - a.SpawnX;
            var dz = a.Z - a.SpawnZ;
            return dx * dx + dz * dz <= radius * radius;
        }
        private void CleanupWaveEnemies(CombatRuntimeState s)
        {
            var ids = s.ActiveActors.Values
                .Where(a => a.Team == 1 && a.Waveindex == s.CurrentWaveIndex)
                .Select(a => a.ActorId)
                .ToList();

            foreach (var id in ids)
                s.ActiveActors.Remove(id);
        }
        private void ResetPlayerPositionsToSpawn(CombatRuntimeState s)
        {
            foreach (var a in s.ActiveActors.Values)
            {
                if (a.Team != 0) continue;      // 플레이어 팀만
                if (a.Dead || a.Hp <= 0) continue;

                a.ReturningToSpawn = true;
                a.ArrivedAtSpawn = false;
                a.TargetActorId = null;
                a.AttackCooldownMs = 0;
                a.SkillCooldownMs = 0;
            }
        }
    }
}
