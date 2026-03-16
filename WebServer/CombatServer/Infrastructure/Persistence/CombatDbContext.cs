using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class CombatDbContext : DbContext
    {
        public CombatDbContext(DbContextOptions<CombatDbContext> options) : base(options) { }

        public DbSet<CombatRecord> Combats => Set<CombatRecord>();
        public DbSet<CombatLogRecord> CombatLogs => Set<CombatLogRecord>();

        // Read-only stage tables (owned by WebServer, queried by CombatServer)
        public DbSet<StageRow> StageRows => Set<StageRow>();
        public DbSet<StageWaveRow> StageWaveRows => Set<StageWaveRow>();
        public DbSet<StageWaveEnemyRow> StageWaveEnemyRows => Set<StageWaveEnemyRow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureCombat(modelBuilder);
            ConfigureCombatLog(modelBuilder);
            ConfigureStageReadModels(modelBuilder);
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
