using Application.Contents.Stages;
using Domain.Combat.Engine;
using Domain.Enum;
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
    public sealed record CombatCommandDto(long ActorId, int SkillId,
    int SkillLevel, long? TargetActorId);

    public sealed record CombatLogSummaryDto(
        long CombatId, int TotalEvents, int DurationMs, int DamageDone, int DamageTaken /* etc */);

    public sealed record StageMasterDto(long StageId, IReadOnlyList<long> EnemyCharacterIds /* ... */);
    public sealed record CharacterMasterDto(long CharacterId, int BaseHp, int BaseAtk, int BaseDef, int BaseAspd /* ... */);
    public sealed record SkillMasterDto(long SkillId, int CooldownMs, float Coeff /* ... */);

    public sealed class CombatSnapshotDto
    {
        public List<ActorSnapshotDto> Actors { get; init; } = new();
    }


    public sealed class ActorSnapshotDto
    {
        public long ActorId { get; init; }
        public float X { get; init; }
        public float Z { get; init; }
        public int Hp { get; init; }
        public bool Dead { get; init; }

    }
    public sealed class CombatTickRequest
    {
        public int Tick { get; set; }
    }
    public sealed class CombatTickResponse
    {
        public long CombatId { get; }
        public int Tick { get; }
        public CombatSnapshotDto Snapshot { get; }
        public IReadOnlyList<CombatLogEventDto> Events { get; }

        public CombatTickResponse(
            long combatId,
            int tick,
            CombatSnapshotDto snapshot,
            IReadOnlyList<CombatLogEventDto> events)
        {
            CombatId = combatId;
            Tick = tick;
            Snapshot = snapshot;
            Events = events ?? Array.Empty<CombatLogEventDto>();
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

    // ── WebServer → CombatServer init payload ────────────────────────────────

    // JSON-serializable actor def (replaces CombatActorDef which has no parameterless ctor)
    public sealed class ActorDefPayload
    {
        public int MasterId { get; init; }
        public bool IsPlayer { get; init; }
        public string ModelKey { get; init; } = "";
        public int MaxHp { get; init; }
        public int Atk { get; init; }
        public int Def { get; init; }
        public int Spd { get; init; }
        public float Range { get; init; }
        public int AttackIntervalMs { get; init; }
        public double CritRate { get; init; }
        public double CritDamage { get; init; }
    }

    public sealed class EnemySpawnPayload
    {
        public int Slot { get; init; }
        public int MonsterId { get; init; }
        public int Level { get; init; }
    }

    public sealed class WaveDefPayload
    {
        public int Index { get; init; }
        public List<EnemySpawnPayload> Enemies { get; init; } = new();
    }

    public sealed class StageDefPayload
    {
        public int StageId { get; init; }
        public List<WaveDefPayload> Waves { get; init; } = new();
    }

    public sealed class PlayerSlotPayload
    {
        public int SlotId { get; init; }
        public long CharacterId { get; init; }
        public int Hp { get; init; }
    }

    public sealed class InitCombatPayload
    {
        public long CombatId { get; init; }
        public int StageId { get; init; }
        public long UserId { get; init; }
        public long Seed { get; init; }
        public List<PlayerSlotPayload> Players { get; init; } = new();
        public StageDefPayload Stage { get; init; } = null!;
        public Dictionary<long, ActorDefPayload> ActorDefs { get; init; } = new();
    }

    // ── CombatServer → WebServer result ──────────────────────────────────────

    public sealed record CombatResultPayload(
        long CombatId,
        int StageId,
        long UserId,
        bool BattleEnded,
        CombatResult? Result,
        int DeadPlayerCount,
        int TotalPlayerCount
    );
}
