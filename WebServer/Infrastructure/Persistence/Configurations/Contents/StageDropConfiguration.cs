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
    public class StageDropConfiguration : IEntityTypeConfiguration<StageDrop>
    {
        public void Configure(EntityTypeBuilder<StageDrop> e) 
        {
            e.ToTable("StageDrops");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

            e.Property(x => x.StageId).HasColumnName("stage_id").IsRequired();
            e.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();

            e.Property(x => x.Rate)
             .HasColumnName("rate")
             .HasColumnType("numeric(6,5)")    // 또는 .HasPrecision(6,5)
             .IsRequired();

            e.Property(x => x.MinQty).HasColumnName("min_qty").HasColumnType("smallint").IsRequired();
            e.Property(x => x.MaxQty).HasColumnName("max_qty").HasColumnType("smallint").IsRequired();

            e.Property(x => x.FirstClearOnly)
             .HasColumnName("first_clear_only")
             .HasDefaultValue(false)
             .IsRequired();

            e.HasIndex(x => x.StageId);
            e.HasIndex(x => x.ItemId);
            // 옵션: e.HasIndex(x => new { x.StageId, x.ItemId, x.FirstClearOnly }).IsUnique();
        }
    }
}
