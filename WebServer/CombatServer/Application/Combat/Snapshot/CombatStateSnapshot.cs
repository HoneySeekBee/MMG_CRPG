using Domain.Combat.Runtime;
using Domain.Enum;

namespace Application.Combat.Snapshot
{
    public sealed class CombatStateSnapshot
    {
        public long CombatId { get; set; }
        public int StageId { get; set; }
        public int UserId { get; set; }
        public long Seed { get; set; }

        public int Tick { get; set; }
        public int SimTimeMs { get; set; }
        public double SimAccumulatorMs { get; set; }
        public CombatSpeed Speed { get; set; }

        public int CurrentWaveIndex { get; set; }
        public int TotalWaves { get; set; }
        public CombatBattlePhase Phase { get; set; }
        public bool BattleEnded { get; set; }
        public CombatResult? Result { get; set; }

        public bool WaitingNextWave { get; set; }
        public int? NextWaveSpawnMs { get; set; }
        public bool WaitingStageResult { get; set; }
        public string? PendingStageResult { get; set; }

        public DateTimeOffset StartedAt { get; set; }

        public List<ActorStateSnapshot> Actors { get; set; } = new();
        public List<PendingSkillCastSnapshot> PendingSkillCasts { get; set; } = new();
        public List<CombatCommandSnapshot> PendingCommands { get; set; } = new();
        public List<ProjectileSnapshot> Projectiles { get; set; } = new();
    }

    public sealed class ActorStateSnapshot
    {
        public long ActorId { get; set; }
        public int Team { get; set; }

        public float X { get; set; }
        public float Z { get; set; }
        public float SpawnX { get; set; }
        public float SpawnZ { get; set; }
        public float FacingX { get; set; }
        public float FacingZ { get; set; }

        public int Hp { get; set; }
        public int HpMax { get; set; }
        public bool Dead { get; set; }
        public bool ReturningToSpawn { get; set; }
        public bool ArrivedAtSpawn { get; set; }

        public int AtkBase { get; set; }
        public int DefBase { get; set; }
        public int SpdBase { get; set; }
        public float RangeBase { get; set; }
        public int AttackIntervalMsBase { get; set; }
        public double CritRateBase { get; set; }
        public double CritDamageBase { get; set; }

        public int SpdEff { get; set; }
        public float RangeEff { get; set; }
        public int AttackCooldownMs { get; set; }
        public int SkillCooldownMs { get; set; }

        public long? TargetActorId { get; set; }
        public int WaveIndex { get; set; }

        public int Shield { get; set; }
        public int ShieldMax { get; set; }

        // CC 상태 및 지속시간
        public bool Stunned { get; set; }
        public bool Silenced { get; set; }
        public bool Frozen { get; set; }
        public bool Rooted { get; set; }
        public bool KnockedDown { get; set; }
        public int StunMs { get; set; }
        public int SilenceMs { get; set; }
        public int FreezeMs { get; set; }
        public int RootMs { get; set; }
        public int KnockdownMs { get; set; }

        // 넉백
        public bool IsKnockbacked { get; set; }
        public float KnockbackVX { get; set; }
        public float KnockbackVZ { get; set; }
        public int KnockbackRemainMs { get; set; }

        // 면역
        public bool ImmuneStun { get; set; }
        public bool ImmuneSilence { get; set; }
        public bool ImmuneFreeze { get; set; }
        public bool ImmuneRoot { get; set; }
        public bool ImmuneKnockdown { get; set; }
        public bool ImmuneKnockback { get; set; }
        public bool ImmuneDebuff { get; set; }

        // 저항
        public float StunResistChance { get; set; }
        public float FreezeResistChance { get; set; }
        public float SilenceResistChance { get; set; }
        public float RootResistChance { get; set; }
        public float KnockbackResistChance { get; set; }
        public float StunDurationReduce { get; set; }
        public float FreezeDurationReduce { get; set; }
        public float SilenceDurationReduce { get; set; }
        public float RootDurationReduce { get; set; }

        // 버프 스탯
        public int BuffAtk { get; set; }
        public int BuffDef { get; set; }
        public float BuffCritRate { get; set; }
        public float BuffCritDamage { get; set; }
        public float BuffDamageReduce { get; set; }
        public float BuffFinalDamageReduce { get; set; }
        public int BuffDefPenFlat { get; set; }
        public float BuffDefPenPercent { get; set; }

        public List<AppliedBuffSnapshot> Buffs { get; set; } = new();
    }

    public sealed class AppliedBuffSnapshot
    {
        public BuffKind Kind { get; set; }
        public int SkillId { get; set; }
        public float Value { get; set; }
        public int DurationMs { get; set; }
        public int MaxDurationMs { get; set; }
        public int Stacks { get; set; }
    }

    public sealed class PendingSkillCastSnapshot
    {
        public long CasterId { get; set; }
        public long? TargetId { get; set; }
        public int SkillId { get; set; }
        public int SkillLevel { get; set; }
        public int DelayMs { get; set; }
        public int HitIndex { get; set; }
        public float ExtraMultiplier { get; set; }
        public List<long> TargetActorIds { get; set; } = new();
    }

    public sealed class CombatCommandSnapshot
    {
        public long ActorId { get; set; }
        public long? TargetActorId { get; set; }
        public int SkillId { get; set; }
        public int SkillLevel { get; set; }
    }

    public sealed class ProjectileSnapshot
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
        public int SkillId { get; set; }   // Effect는 복구 시 스킬 캐시에서 재구성

        public bool Tracking { get; set; }
        public bool Piercing { get; set; }
        public float AoeRadius { get; set; }
        public int MaxHitCount { get; set; }
        public int ChainCount { get; set; }
        public float ChainRange { get; set; }
        public int BounceCount { get; set; }
        public float BounceRange { get; set; }

        public List<long> HitActors { get; set; } = new();
    }
}
