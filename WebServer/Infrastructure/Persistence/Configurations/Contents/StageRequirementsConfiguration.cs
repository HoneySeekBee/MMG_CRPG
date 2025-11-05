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
    public class StageRequirementsConfiguration : IEntityTypeConfiguration<StageRequirement>
    { 
        public void Configure(EntityTypeBuilder<StageRequirement> e) 
        {
            e.ToTable("StageRequirements");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

            e.Property(x => x.StageId).HasColumnName("stage_id").IsRequired();
            e.Property(x => x.RequiredStageId).HasColumnName("required_stage_id").IsRequired(false);
            e.Property(x => x.MinAccountLevel).HasColumnName("min_account_level").HasColumnType("smallint").IsRequired(false);

            e.HasIndex(x => x.StageId);
            e.HasIndex(x => x.RequiredStageId);

            e.HasOne<Stage>()                      // self-ref: RequiredStageId
             .WithMany()
             .HasForeignKey(x => x.RequiredStageId)
             .OnDelete(DeleteBehavior.SetNull);    // 운영 중 삭제 안전

            // 부모 Stage와의 관계는 Stage에서 Cascade already
        }

    }
}
