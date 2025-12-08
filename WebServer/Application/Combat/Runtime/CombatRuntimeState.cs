using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Application.Contents.Stages;
using Application.SkillLevels;
using Application.Skills;
using Domain.Enum;
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
        public Queue<PendingSkillCast> PendingSkillCasts { get; } = new();
        public Dictionary<int, SkillWithLevelsDto> SkillMaster { get; init; } = new();


        public int NowMs()
        {
            return (int)(DateTimeOffset.UtcNow - StartedAt).TotalMilliseconds;
        }
    }
    public enum CombatBattlePhase
    {
        Running,            // 전투 진행 중
        WaitingNextWave,    // 웨이브 끝, 클라 신호 기다리는 중
        Completed           // 스테이지 전체 종료
    }
    public sealed record PendingSkillCast
    {
        public long CasterId { get; init; }
        public long? TargetId { get; init; }
        public int SkillId { get; init; }
        public int SkillLevel { get; init; }

        public int DelayMs { get; set; } = 0;
        public float ExtraMultiplier { get; set; } = 1.0f;
    }
    public sealed class CombatSkillDef
    {
        public int SkillId { get; init; }
        public SkillType Type { get; init; }
        public TargetSideType TargetSide { get; init; }
        public SkillTargetingType TargetingType { get; init; }
        public AoeShapeType AoeShape { get; init; }
        public JsonNode? BaseInfo { get; init; }
        public List<SkillLevelDto> Levels { get; init; }
    }

}
