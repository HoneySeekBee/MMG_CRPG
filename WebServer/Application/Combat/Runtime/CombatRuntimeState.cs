using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Application.Contents.Stages;
using Application.SkillLevels;
using Application.Skills;
using Domain.Entities.Skill;
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
        public List<ProjectileState> Projectiles { get; set; } = new();


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
        public int HitIndex { get; set; } = 0;
        public float ExtraMultiplier { get; set; } = 1.0f;
        public List<long> TargetActorIds { get; set; } = new();
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
    public class ProjectileState
    {
        public long Id { get; set; }

        public long CasterId { get; set; }
        public long? TargetId { get; set; }

        public float X { get; set; }
        public float Z { get; set; }

        public float VX { get; set; }
        public float VZ { get; set; }
        public float Speed { get; set; }

        public int LifetimeMs { get; set; }

        public int SkillId { get; set; }
        public SkillEffect Effect { get; set; }
         
        // 확장 기능 옵션들 
        public bool Tracking { get; set; }

        // 관통(맞아도 삭제 X)
        public bool Piercing { get; set; } = false;

        // 폭발 범위(>0이면 AoE)
        public float AoeRadius { get; set; } = 0f;

        // 몇 명까지 때릴 수 있는지 (0이면 무제한)
        public int MaxHitCount { get; set; } = 1;

        // 체인 공격
        public int ChainCount { get; set; } = 0;
        public float ChainRange { get; set; } = 0f;

        // 튕김
        public int BounceCount { get; set; } = 0;
        public float BounceRange { get; set; } = 0f;

        // 중복 피격 방지용
        public HashSet<long> HitActors { get; set; } = new();
    }
}
