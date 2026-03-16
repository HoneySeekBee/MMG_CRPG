using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class CombatDbContext : DbContext
    {
        public CombatDbContext(DbContextOptions<CombatDbContext> options) : base(options) { }

        public DbSet<CombatRecord> Combats => Set<CombatRecord>();
        public DbSet<CombatLogRecord> CombatLogs => Set<CombatLogRecord>();

        // Read-only stage tables
        public DbSet<StageRow> StageRows => Set<StageRow>();
        public DbSet<StageWaveRow> StageWaveRows => Set<StageWaveRow>();
        public DbSet<StageWaveEnemyRow> StageWaveEnemyRows => Set<StageWaveEnemyRow>();

        // Read-only master data tables
        public DbSet<CharacterRow> CharacterRows => Set<CharacterRow>();
        public DbSet<CharacterStatRow> CharacterStatRows => Set<CharacterStatRow>();
        public DbSet<SkillRow> SkillRows => Set<SkillRow>();
        public DbSet<UserCharacterRow> UserCharacterRows => Set<UserCharacterRow>();
        public DbSet<MonsterStatRow> MonsterStatRows => Set<MonsterStatRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureCombat(modelBuilder);
            ConfigureCombatLog(modelBuilder);
            ConfigureStageReadModels(modelBuilder);
            ConfigureMasterDataReadModels(modelBuilder);
        }

        private static void ConfigureCombat(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<CombatRecord>();
            e.ToTable("Combat");
            e.HasKey(x => x.Id);

            e.Property(x => x.Mode)
                .IsRequired()
                .HasColumnName("Mode");

            e.Property(x => x.StageId)
                .HasColumnName("StageId");

            e.Property(x => x.Seed)
                .IsRequired()
                .HasColumnName("Seed");

            e.Property(x => x.InputJson)
                .IsRequired()
                .HasColumnName("InputJson")
                .HasColumnType("jsonb");

            e.Property(x => x.Result)
                .HasColumnName("Result");

            e.Property(x => x.ClearMs)
                .HasColumnName("Clear_ms");

            e.Property(x => x.BalanceVersion)
                .HasColumnName("BalanceVersion");

            e.Property(x => x.ClientVersion)
                .HasColumnName("ClientVersion");

            e.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt");

            e.HasIndex(x => x.StageId);
            e.HasIndex(x => x.Mode);
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_combat_created_at");
        }

        private static void ConfigureStageReadModels(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StageRow>(e =>
            {
                e.HasNoKey().ToTable("Stages");
                e.Property(x => x.Id).HasColumnName("stage_id");
                e.Property(x => x.Chapter).HasColumnName("chapter_id");
                e.Property(x => x.StageNum).HasColumnName("stage_num");
                e.Property(x => x.Name).HasColumnName("name");
                e.Property(x => x.RecommendedPower).HasColumnName("recommended_power");
                e.Property(x => x.StaminaCost).HasColumnName("stamina_cost");
                e.Property(x => x.IsActive).HasColumnName("is_active");
            });

            modelBuilder.Entity<StageWaveRow>(e =>
            {
                e.HasNoKey().ToTable("StageWaves");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.StageId).HasColumnName("stage_id");
                e.Property(x => x.Index).HasColumnName("index");
                e.Property(x => x.BatchNum).HasColumnName("batch_num");
            });

            modelBuilder.Entity<StageWaveEnemyRow>(e =>
            {
                e.HasNoKey().ToTable("StageWaveEnemies");
                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.StageWaveId).HasColumnName("stage_wave_id");
                e.Property(x => x.EnemyCharacterId).HasColumnName("enemy_character_id");
                e.Property(x => x.Level).HasColumnName("level");
                e.Property(x => x.Slot).HasColumnName("slot");
                e.Property(x => x.AiProfile).HasColumnName("ai_profile");
            });
        }

        private static void ConfigureMasterDataReadModels(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CharacterRow>(e =>
            {
                e.HasNoKey().ToTable("Characters");
                e.Property(x => x.Id).HasColumnName("CharacterId");
            });

            modelBuilder.Entity<CharacterStatRow>(e =>
            {
                e.HasNoKey().ToTable("CharacterStatProgression");
                e.Property(x => x.CharacterId).HasColumnName("CharacterId");
                e.Property(x => x.Level).HasColumnName("Level");
                e.Property(x => x.HP).HasColumnName("HP");
                e.Property(x => x.ATK).HasColumnName("ATK");
                e.Property(x => x.DEF).HasColumnName("DEF");
                e.Property(x => x.SPD).HasColumnName("SPD");
                e.Property(x => x.CriRate).HasColumnName("CriRate");
                e.Property(x => x.CriDamage).HasColumnName("CriDamage");
                e.Property(x => x.Range).HasColumnName("Range");
            });

            modelBuilder.Entity<SkillRow>(e =>
            {
                e.HasNoKey().ToTable("Skills");
                e.Property(x => x.SkillId).HasColumnName("SkillId");
                e.Property(x => x.Type).HasColumnName("Type");
                e.Property(x => x.TargetingType).HasColumnName("TargetingType");
                e.Property(x => x.AoeShape).HasColumnName("AoeShape");
                e.Property(x => x.TargetSide).HasColumnName("TargetSide");
                e.Property(x => x.BaseInfo).HasColumnName("Meta").HasColumnType("jsonb");
            });

            modelBuilder.Entity<UserCharacterRow>(e =>
            {
                e.HasNoKey().ToTable("UserCharacters");
                e.Property(x => x.UserCharacterId).HasColumnName("user_character_id");
                e.Property(x => x.UserId).HasColumnName("UserId");
                e.Property(x => x.CharacterId).HasColumnName("CharacterId");
                e.Property(x => x.Level).HasColumnName("Level");
            });

            modelBuilder.Entity<MonsterStatRow>(e =>
            {
                e.HasNoKey().ToTable("MonsterStatProgression");
                e.Property(x => x.MonsterId).HasColumnName("monster_id");
                e.Property(x => x.Level).HasColumnName("level");
                e.Property(x => x.HP).HasColumnName("hp");
                e.Property(x => x.ATK).HasColumnName("atk");
                e.Property(x => x.DEF).HasColumnName("def");
                e.Property(x => x.SPD).HasColumnName("spd");
                e.Property(x => x.CritRate).HasColumnName("cri_rate");
                e.Property(x => x.CritDamage).HasColumnName("cri_damage");
                e.Property(x => x.Range).HasColumnName("range");
            });
        }

        private static void ConfigureCombatLog(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<CombatLogRecord>();
            e.ToTable("CombatLog");
            e.HasKey(x => x.Id);

            e.Property(x => x.CombatId)
                .IsRequired()
                .HasColumnName("CombatId");

            e.Property(x => x.TMs)
                .IsRequired()
                .HasColumnName("t_ms");

            e.Property(x => x.PayloadJson)
                .IsRequired()
                .HasColumnName("PayloadJson")
                .HasColumnType("jsonb");

            e.HasIndex(x => new { x.CombatId, x.TMs })
                .HasDatabaseName("idx_combat_log_order");

            e.HasOne(x => x.Combat)
                .WithMany(c => c.Logs)
                .HasForeignKey(x => x.CombatId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
