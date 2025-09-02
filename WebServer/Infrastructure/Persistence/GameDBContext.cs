using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Infrastructure.Persistence
{
    public class GameDBContext : DbContext
    {
        public GameDBContext(DbContextOptions<GameDBContext> options) : base(options) { }

        public DbSet<Icon> Icons => Set<Icon>();
        public DbSet<Element> Elements => Set<Element>();
        public DbSet<ElementAffinity> ElementAffinities => Set<ElementAffinity>();

        public DbSet<Faction> Factions => Set<Faction>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Rarity> Rarities => Set<Rarity>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Console.WriteLine("OnModelCreateing");
            Modeling_Icon(modelBuilder);
            Modeling_Element(modelBuilder);
            Modeling_ElementAffinity(modelBuilder);
            Modeling_Faction(modelBuilder);
            Modeling_Role(modelBuilder);
            Modeling_Rarity(modelBuilder);
        }

        private void Modeling_Icon(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Icon>(e =>
            {
                e.ToTable("Icons"); // 테이블 명
                e.HasKey(x => x.IconId);
                e.Property(x => x.Key).IsRequired();
                e.HasIndex(x => x.Key).IsUnique();
            });
        }
        private void Modeling_Element(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Element>(e =>
            {
                e.ToTable("Element");
                e.HasKey(x => x.ElementId);
                e.Property(x => x.Key).IsRequired();
                e.Property(x => x.Label).IsRequired();
                e.Property(x => x.ColorHex).IsRequired();
                e.Property(x => x.SortOrder).HasColumnType("smallint");
                e.Property(x => x.Meta).HasColumnType("jsonb"); // PostgreSQL
                e.HasIndex(x => new { x.IsActive, x.SortOrder });
                e.HasIndex(x => x.Key).IsUnique();
            });
        }
        public void Modeling_ElementAffinity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ElementAffinity>(e =>
            {
                e.ToTable("ElementAffinity");
                e.HasKey(x => new { x.AttackerElementId, x.DefenderElementId });
                e.Property(x => x.Multiplier)
                .HasColumnType("numeric(4,2)")
                .HasDefaultValue(1.00m);
            });
        }

        public void Modeling_Faction(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Faction>(e =>
            {
                e.ToTable("Faction");
                e.HasKey(x => x.FactionId);
                e.Property(x => x.FactionId).ValueGeneratedOnAdd();
                e.Property(x => x.Key).IsRequired();
                e.Property(x => x.Label).IsRequired();
                e.Property(x => x.ColorHex);
                e.Property(x => x.Meta).HasColumnType("jsonb");      // pg jsonb
                e.Property(x => x.IsActive).HasDefaultValue(true);

                e.HasIndex(x => x.Key).IsUnique();               
            });
        }
        public void Modeling_Role(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("Role");
                e.HasKey(x => x.RoleId);
                e.Property(x => x.RoleId).ValueGeneratedOnAdd();
                e.Property(x => x.Key).IsRequired();
                e.Property(x => x.Label).IsRequired();
                e.Property(x => x.ColorHex);
                e.Property(x => x.Meta).HasColumnType("jsonb");
                e.Property(x => x.IsActive).HasDefaultValue(true);

                e.HasIndex(x => x.Key).IsUnique();
            });
        }

        public void Modeling_Rarity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rarity>(e =>
            {
                e.ToTable("Rarity");
                e.HasKey(x => x.RarityId);
                e.Property(x => x.RarityId).ValueGeneratedOnAdd();
                e.Property(x => x.Stars).IsRequired();           
                e.Property(x => x.Key).IsRequired();
                e.Property(x => x.Label).IsRequired();
                e.Property(x => x.ColorHex);
                e.Property(x => x.Meta).HasColumnType("jsonb");
                e.Property(x => x.IsActive).HasDefaultValue(true);

                e.HasIndex(x => x.Key).IsUnique();
            });
        }
    }
}
