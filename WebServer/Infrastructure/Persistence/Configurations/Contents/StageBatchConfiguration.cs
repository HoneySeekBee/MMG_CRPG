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
    public sealed class StageBatchConfiguration : IEntityTypeConfiguration<StageBatch>
    {
        public void Configure(EntityTypeBuilder<StageBatch> b)
        {
            b.ToTable("StageBatches");

            b.HasKey(x => x.Id);

            b.Property(x => x.Id)
                .HasColumnName("stage_batch_id")
                .ValueGeneratedOnAdd();

            b.Property(x => x.StageId)
                .HasColumnName("stage_id")
                .IsRequired();

            b.Property(x => x.BatchNum)
                .HasColumnName("batch_num")
                .IsRequired();

            b.Property(x => x.UnitKey)
                .HasColumnName("unit_key")
                .HasMaxLength(100)
                .IsRequired();

            b.Property(x => x.EnvKey)
                .HasColumnName("env_key")
                .HasMaxLength(100)
                .IsRequired();

            b.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            b.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 관계 설정 (Stage 1 : N StageBatches)
            b.HasOne<Stage>()
                .WithMany(s => s.Batches)
                .HasForeignKey(x => x.StageId)
                .OnDelete(DeleteBehavior.Cascade);

            // 유니크 인덱스: 같은 Stage 내에서 BatchNum은 고유해야 함
            b.HasIndex(x => new { x.StageId, x.BatchNum })
                .IsUnique();
             
        }
    }
} 
