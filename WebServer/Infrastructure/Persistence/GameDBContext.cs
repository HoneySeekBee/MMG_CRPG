using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Infrastructure.Persistence
{
    public class GameDBContext : DbContext
    {
        public GameDBContext(DbContextOptions<GameDBContext> options) : base(options) { }

        public DbSet<Icon> Icons => Set<Icon>();
        public DbSet<Element> Elements => Set<Element>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Console.WriteLine("OnModelCreateing");
            Modeling_Icon(modelBuilder);
            Modeling_Element(modelBuilder);
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
    }
}
