using Domain.Entities.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Contents
{
    public class StageConfiguration : IEntityTypeConfiguration<Stage>
    {
        public void Configure(EntityTypeBuilder<Stage> e) 
        {
            e.ToTable("Stages");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("stage_id").ValueGeneratedOnAdd();

            e.Property(x => x.Chapter).HasColumnName("chapter_id").IsRequired();
            e.Property(x => x.StageNumber).HasColumnName("stage_num").IsRequired();

            e.Property(x => x.Name)
             .HasColumnName("name")
             .HasMaxLength(64)            // 요구 시 50으로 줄여도 OK
             .IsRequired(false);

            e.Property(x => x.RecommendedPower)
             .HasColumnName("recommended_power")
             .HasColumnType("smallint")
             .IsRequired();

            e.Property(x => x.StaminaCost)
             .HasColumnName("stamina_cost")
             .HasColumnType("smallint")
             .IsRequired();

            e.Property(x => x.IsActive)
             .HasColumnName("is_active")
             .HasDefaultValue(true)
             .IsRequired();

            // 유니크: (Chapter, Order)
            e.HasIndex(x => new { x.Chapter, x.StageNumber }).IsUnique();

            // 관계 (Cascade)
            e.HasMany(x => x.Waves)
             .WithOne()
             .HasForeignKey(w => w.StageId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Drops)
             .WithOne()
             .HasForeignKey(d => d.StageId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.FirstRewards)
             .WithOne()
             .HasForeignKey(r => r.StageId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Requirements)
             .WithOne()
             .HasForeignKey(r => r.StageId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
