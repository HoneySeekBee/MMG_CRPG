using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class CombatDbContext : DbContext
    {
        public CombatDbContext(DbContextOptions<CombatDbContext> options) : base(options) { }

        public DbSet<CombatRecord> Combats => Set<CombatRecord>();
        public DbSet<CombatLogRecord> CombatLogs => Set<CombatLogRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureCombat(modelBuilder);
            ConfigureCombatLog(modelBuilder);
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
