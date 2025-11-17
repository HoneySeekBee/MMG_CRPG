using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;using Application.Contents.Stages;

namespace Application.Combat.Runtime
{
    public sealed class CombatRuntimeState
    { 
        public int Tick { get; set; } = 0;
        public long CombatId { get; init; }
        public int StageId { get; init; }
        public int UserId { get; set; }
        public long Seed { get; init; }

        public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;

        public int CurrentWaveIndex { get; set; } = 1;
        public int TotalWaves { get; set; }
        public bool BattleEnded { get; set; } = false;

        public CombatBattlePhase Phase { get; set; } = CombatBattlePhase.Running;
        public Queue<CombatCommandDto> PendingCommands { get; } = new();
        public CombatRuntimeSnapshot Snapshot { get; set; }
        public MasterPackDto MasterPack { get; set; }
        public object SyncRoot { get; } = new();
        public CombatStageDef StageDef { get; }
        public Dictionary<long, ActorState> ActiveActors { get; } = new();
        public bool WaitingNextWave { get; set; }
        public int? NextWaveSpawnMs { get; set; }
    }
    public enum CombatBattlePhase
    {
        Running,            // 전투 진행 중
        WaitingNextWave,    // 웨이브 끝, 클라 신호 기다리는 중
        Completed           // 스테이지 전체 종료
    }
    

}
