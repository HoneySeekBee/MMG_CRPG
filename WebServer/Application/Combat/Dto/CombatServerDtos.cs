using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Combat.Dto
{
    public sealed class CombatInitialSnapshotPayload
    {
        public List<ActorInitPayload> Actors { get; init; } = new();
    }

    public sealed class ActorInitPayload
    {
        public long ActorId { get; init; }
        public int Team { get; init; }
        public float X { get; init; }
        public float Z { get; init; }
        public int Hp { get; init; }
        public int WaveIndex { get; init; }
        public long MasterId { get; init; }
    }

    public sealed record CombatResultPayload(
        long CombatId,
        int StageId,
        long UserId,
        bool BattleEnded,
        Domain.Enum.CombatResult? Result,
        int DeadPlayerCount,
        int TotalPlayerCount
    );

    // Payload WebServer sends to CombatServer /combat/init

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

    public sealed class PlayerSlotPayload
    {
        public int SlotId { get; init; }
        public long CharacterId { get; init; }
        public int Hp { get; init; }
    }

    public sealed class StageDefPayload
    {
        public int StageId { get; init; }
        public List<WaveDefPayload> Waves { get; init; } = new();
    }

    public sealed class WaveDefPayload
    {
        public int Index { get; init; }
        public List<EnemySpawnPayload> Enemies { get; init; } = new();
    }

    public sealed class EnemySpawnPayload
    {
        public int Slot { get; init; }
        public int MonsterId { get; init; }
        public int Level { get; init; }
    }

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
}
