using Application.Contents.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat
{
    public sealed record CombatLogEventDto(
        int TMs, string Type, string Actor, string? Target, int? Damage, bool? Crit,
        IReadOnlyDictionary<string, object?>? Extra);

    // 로그 페이징
    public sealed record CombatLogPageDto(
        long CombatId,
        IReadOnlyList<CombatLogEventDto> Items,
        string? NextCursor
    ); 
    public sealed record ActorInitDto(
        long ActorId,
        int Team,          // 0 = Player, 1 = Enemy (enum으로 빼도 됨)
        float X,
        float Z,
        int Hp, 
        int WaveIndex,
        long MasterId
    );

    // 전투 시작 시 전체 스냅샷
    public sealed record CombatInitialSnapshotDto(
        IReadOnlyList<ActorInitDto> Actors
    );
    public sealed record CombatCommandDto(long ActorId, long SkillId, long? TargetActorId);

    public sealed record CombatLogSummaryDto(
        long CombatId, int TotalEvents, int DurationMs, int DamageDone, int DamageTaken /* etc */);

    public sealed record StageMasterDto(long StageId, IReadOnlyList<long> EnemyCharacterIds /* ... */);
    public sealed record CharacterMasterDto(long CharacterId, int BaseHp, int BaseAtk, int BaseDef, int BaseAspd /* ... */);
    public sealed record SkillMasterDto(long SkillId, int CooldownMs, float Coeff /* ... */);

 

    public sealed record CombatSnapshotDto(
        IReadOnlyList<ActorSnapshotDto> Actors
    );

    public sealed record ActorSnapshotDto(
        long ActorId,
        float X,
        float Z,
        int Hp,
        bool Dead,
        IReadOnlyList<CombatLogEventDto> Events
    ); 
    public sealed class CombatTickRequest
    {
        public int Tick { get; set; }
    }

    public sealed class CombatTickResponse
    {
        public long CombatId { get; }
        public int Tick { get; }
        public CombatSnapshotDto Snapshot { get; }

        public CombatTickResponse(long combatId, int tick, CombatSnapshotDto snapshot)
        {
            CombatId = combatId;
            Tick = tick;
            Snapshot = snapshot;
        }
    }
    public sealed class MasterPackDto
    {
        public CombatStageDef Stage { get; init; }
        public Dictionary<long, CombatActorDef> Actors { get; init; }

        public MasterPackDto(
            CombatStageDef stage,
            Dictionary<long, CombatActorDef> actors)
        {
            Stage = stage;
            Actors = actors;
        }
    }
}
