using Application.Skills;
using Domain.Combat.Runtime;
using Domain.Entities.Combats;

namespace Application.Combat.Snapshot
{
    public static class CombatStateSerializer
    {
        public static CombatStateSnapshot ToSnapshot(CombatRuntimeState s)
        {
            return new CombatStateSnapshot
            {
                CombatId = s.CombatId,
                StageId = s.StageId,
                UserId = s.UserId,
                Seed = s.Seed,

                Tick = s.Tick,
                SimTimeMs = s.SimTimeMs,
                SimAccumulatorMs = s.SimAccumulatorMs,
                Speed = s.Speed,

                CurrentWaveIndex = s.CurrentWaveIndex,
                TotalWaves = s.TotalWaves,
                Phase = s.Phase,
                BattleEnded = s.BattleEnded,
                Result = s.Result,

                WaitingNextWave = s.WaitingNextWave,
                NextWaveSpawnMs = s.NextWaveSpawnMs,
                WaitingStageResult = s.WaitingStageResult,
                PendingStageResult = s.PendingStageResult,

                StartedAt = s.StartedAt,

                Actors = s.ActiveActors.Values.Select(ToActorSnapshot).ToList(),
                PendingSkillCasts = s.PendingSkillCasts.Select(ToPendingSkillCastSnapshot).ToList(),
                PendingCommands = s.PendingCommands.Select(ToCombatCommandSnapshot).ToList(),
                Projectiles = s.Projectiles.Select(ToProjectileSnapshot).ToList(),
            };
        }

        public static void RestoreInto(CombatStateSnapshot snap, CombatRuntimeState s, ISkillCache skillCache)
        {
            s.Tick = snap.Tick;
            s.SimTimeMs = snap.SimTimeMs;
            s.SimAccumulatorMs = snap.SimAccumulatorMs;
            s.Speed = snap.Speed;

            s.CurrentWaveIndex = snap.CurrentWaveIndex;
            s.TotalWaves = snap.TotalWaves;
            s.Phase = snap.Phase;
            s.BattleEnded = snap.BattleEnded;
            s.Result = snap.Result;

            s.WaitingNextWave = snap.WaitingNextWave;
            s.NextWaveSpawnMs = snap.NextWaveSpawnMs;
            s.WaitingStageResult = snap.WaitingStageResult;
            s.PendingStageResult = snap.PendingStageResult;

            s.ActiveActors.Clear();
            foreach (var a in snap.Actors)
                s.ActiveActors[a.ActorId] = ToActorState(a);

            s.PendingSkillCasts.Clear();
            foreach (var p in snap.PendingSkillCasts)
                s.PendingSkillCasts.Enqueue(ToPendingSkillCast(p));

            s.PendingCommands.Clear();
            foreach (var c in snap.PendingCommands)
                s.PendingCommands.Enqueue(ToCombatCommand(c));

            s.Projectiles.Clear();
            foreach (var p in snap.Projectiles)
                s.Projectiles.Add(ToProjectileState(p, skillCache));
        }

        // ── Actor ────────────────────────────────────────────────────────────

        private static ActorStateSnapshot ToActorSnapshot(ActorState a) => new()
        {
            ActorId = a.ActorId,
            Team = a.Team,
            X = a.X,
            Z = a.Z,
            SpawnX = a.SpawnX,
            SpawnZ = a.SpawnZ,
            FacingX = a.FacingX,
            FacingZ = a.FacingZ,
            Hp = a.Hp,
            HpMax = a.HpMax,
            Dead = a.Dead,
            ReturningToSpawn = a.ReturningToSpawn,
            ArrivedAtSpawn = a.ArrivedAtSpawn,
            AtkBase = a.AtkBase,
            DefBase = a.DefBase,
            SpdBase = a.SpdBase,
            RangeBase = a.RangeBase,
            AttackIntervalMsBase = a.AttackIntervalMsBase,
            CritRateBase = a.CritRateBase,
            CritDamageBase = a.CritDamageBase,
            SpdEff = a.SpdEff,
            RangeEff = a.RangeEff,
            AttackCooldownMs = a.AttackCooldownMs,
            SkillCooldownMs = a.SkillCooldownMs,
            TargetActorId = a.TargetActorId,
            WaveIndex = a.Waveindex,
            Shield = a.Shield,
            ShieldMax = a.ShieldMax,
            Stunned = a.Stunned,
            Silenced = a.Silenced,
            Frozen = a.Frozen,
            Rooted = a.Rooted,
            KnockedDown = a.KnockedDown,
            StunMs = a.StunMs,
            SilenceMs = a.SilenceMs,
            FreezeMs = a.FreezeMs,
            RootMs = a.RootMs,
            KnockdownMs = a.KnockdownMs,
            IsKnockbacked = a.IsKnockbacked,
            KnockbackVX = a.KnockbackVX,
            KnockbackVZ = a.KnockbackVZ,
            KnockbackRemainMs = a.KnockbackRemainMs,
            ImmuneStun = a.ImmuneStun,
            ImmuneSilence = a.ImmuneSilence,
            ImmuneFreeze = a.ImmuneFreeze,
            ImmuneRoot = a.ImmuneRoot,
            ImmuneKnockdown = a.ImmuneKnockdown,
            ImmuneKnockback = a.ImmuneKnockback,
            ImmuneDebuff = a.ImmuneDebuff,
            StunResistChance = a.StunResistChance,
            FreezeResistChance = a.FreezeResistChance,
            SilenceResistChance = a.SilenceResistChance,
            RootResistChance = a.RootResistChance,
            KnockbackResistChance = a.KnockbackResistChance,
            StunDurationReduce = a.StunDurationReduce,
            FreezeDurationReduce = a.FreezeDurationReduce,
            SilenceDurationReduce = a.SilenceDurationReduce,
            RootDurationReduce = a.RootDurationReduce,
            BuffAtk = a.BuffAtk,
            BuffDef = a.BuffDef,
            BuffCritRate = a.BuffCritRate,
            BuffCritDamage = a.BuffCritDamage,
            BuffDamageReduce = a.BuffDamageReduce,
            BuffFinalDamageReduce = a.BuffFinalDamageReduce,
            BuffDefPenFlat = a.BuffDefPenFlat,
            BuffDefPenPercent = a.BuffDefPenPercent,
            Buffs = a.Buffs.Select(b => new AppliedBuffSnapshot
            {
                Kind = b.Kind,
                SkillId = b.SkillId,
                Value = b.Value,
                DurationMs = b.DurationMs,
                MaxDurationMs = b.MaxDurationMs,
                Stacks = b.Stacks,
            }).ToList(),
        };

        private static ActorState ToActorState(ActorStateSnapshot s)
        {
            var a = new ActorState
            {
                ActorId = s.ActorId,
                Team = s.Team,
                X = s.X,
                Z = s.Z,
                SpawnX = s.SpawnX,
                SpawnZ = s.SpawnZ,
                FacingX = s.FacingX,
                FacingZ = s.FacingZ,
                Hp = s.Hp,
                HpMax = s.HpMax,
                Dead = s.Dead,
                ReturningToSpawn = s.ReturningToSpawn,
                ArrivedAtSpawn = s.ArrivedAtSpawn,
                AtkBase = s.AtkBase,
                DefBase = s.DefBase,
                SpdBase = s.SpdBase,
                RangeBase = s.RangeBase,
                AttackIntervalMsBase = s.AttackIntervalMsBase,
                CritRateBase = s.CritRateBase,
                CritDamageBase = s.CritDamageBase,
                SpdEff = s.SpdEff,
                RangeEff = s.RangeEff,
                AttackCooldownMs = s.AttackCooldownMs,
                SkillCooldownMs = s.SkillCooldownMs,
                TargetActorId = s.TargetActorId,
                Waveindex = s.WaveIndex,
                Shield = s.Shield,
                ShieldMax = s.ShieldMax,
                Stunned = s.Stunned,
                Silenced = s.Silenced,
                Frozen = s.Frozen,
                Rooted = s.Rooted,
                KnockedDown = s.KnockedDown,
                StunMs = s.StunMs,
                SilenceMs = s.SilenceMs,
                FreezeMs = s.FreezeMs,
                RootMs = s.RootMs,
                KnockdownMs = s.KnockdownMs,
                IsKnockbacked = s.IsKnockbacked,
                KnockbackVX = s.KnockbackVX,
                KnockbackVZ = s.KnockbackVZ,
                KnockbackRemainMs = s.KnockbackRemainMs,
                ImmuneStun = s.ImmuneStun,
                ImmuneSilence = s.ImmuneSilence,
                ImmuneFreeze = s.ImmuneFreeze,
                ImmuneRoot = s.ImmuneRoot,
                ImmuneKnockdown = s.ImmuneKnockdown,
                ImmuneKnockback = s.ImmuneKnockback,
                ImmuneDebuff = s.ImmuneDebuff,
                StunResistChance = s.StunResistChance,
                FreezeResistChance = s.FreezeResistChance,
                SilenceResistChance = s.SilenceResistChance,
                RootResistChance = s.RootResistChance,
                KnockbackResistChance = s.KnockbackResistChance,
                StunDurationReduce = s.StunDurationReduce,
                FreezeDurationReduce = s.FreezeDurationReduce,
                SilenceDurationReduce = s.SilenceDurationReduce,
                RootDurationReduce = s.RootDurationReduce,
                BuffAtk = s.BuffAtk,
                BuffDef = s.BuffDef,
                BuffCritRate = s.BuffCritRate,
                BuffCritDamage = s.BuffCritDamage,
                BuffDamageReduce = s.BuffDamageReduce,
                BuffFinalDamageReduce = s.BuffFinalDamageReduce,
                BuffDefPenFlat = s.BuffDefPenFlat,
                BuffDefPenPercent = s.BuffDefPenPercent,
            };

            foreach (var b in s.Buffs)
                a.Buffs.Add(new Domain.Combat.Runtime.AppliedBuff
                {
                    Kind = b.Kind,
                    SkillId = b.SkillId,
                    Value = b.Value,
                    DurationMs = b.DurationMs,
                    MaxDurationMs = b.MaxDurationMs,
                    Stacks = b.Stacks,
                });

            return a;
        }

        // ── PendingSkillCast ─────────────────────────────────────────────────

        private static PendingSkillCastSnapshot ToPendingSkillCastSnapshot(PendingSkillCast p) => new()
        {
            CasterId = p.CasterId,
            TargetId = p.TargetId,
            SkillId = p.SkillId,
            SkillLevel = p.SkillLevel,
            DelayMs = p.DelayMs,
            HitIndex = p.HitIndex,
            ExtraMultiplier = p.ExtraMultiplier,
            TargetActorIds = p.TargetActorIds.ToList(),
        };

        private static PendingSkillCast ToPendingSkillCast(PendingSkillCastSnapshot s) => new()
        {
            CasterId = s.CasterId,
            TargetId = s.TargetId,
            SkillId = s.SkillId,
            SkillLevel = s.SkillLevel,
            DelayMs = s.DelayMs,
            HitIndex = s.HitIndex,
            ExtraMultiplier = s.ExtraMultiplier,
            TargetActorIds = s.TargetActorIds.ToList(),
        };

        // ── CombatCommand ────────────────────────────────────────────────────

        private static CombatCommandSnapshot ToCombatCommandSnapshot(CombatCommand c) => new()
        {
            ActorId = c.ActorId,
            TargetActorId = c.TargetActorId,
            SkillId = c.SkillId,
            SkillLevel = c.SkillLevel,
        };

        private static CombatCommand ToCombatCommand(CombatCommandSnapshot s) =>
            new(s.ActorId, s.TargetActorId, s.SkillId, s.SkillLevel);

        // ── Projectile ───────────────────────────────────────────────────────

        private static ProjectileSnapshot ToProjectileSnapshot(ProjectileState p) => new()
        {
            Id = p.Id,
            CasterId = p.CasterId,
            TargetId = p.TargetId,
            X = p.X,
            Z = p.Z,
            VX = p.VX,
            VZ = p.VZ,
            Speed = p.Speed,
            LifetimeMs = p.LifetimeMs,
            SkillId = p.SkillId,
            Tracking = p.Tracking,
            Piercing = p.Piercing,
            AoeRadius = p.AoeRadius,
            MaxHitCount = p.MaxHitCount,
            ChainCount = p.ChainCount,
            ChainRange = p.ChainRange,
            BounceCount = p.BounceCount,
            BounceRange = p.BounceRange,
            HitActors = p.HitActors.ToList(),
        };

        private static ProjectileState ToProjectileState(ProjectileSnapshot s, ISkillCache skillCache) => new()
        {
            Id = s.Id,
            CasterId = s.CasterId,
            TargetId = s.TargetId,
            X = s.X,
            Z = s.Z,
            VX = s.VX,
            VZ = s.VZ,
            Speed = s.Speed,
            LifetimeMs = s.LifetimeMs,
            SkillId = s.SkillId,
            Effect = skillCache.GetById(s.SkillId)?.Effect ?? throw new InvalidOperationException($"Skill {s.SkillId} not found in cache."),
            Tracking = s.Tracking,
            Piercing = s.Piercing,
            AoeRadius = s.AoeRadius,
            MaxHitCount = s.MaxHitCount,
            ChainCount = s.ChainCount,
            ChainRange = s.ChainRange,
            BounceCount = s.BounceCount,
            BounceRange = s.BounceRange,
            HitActors = s.HitActors.ToHashSet(),
        };
    }
}
