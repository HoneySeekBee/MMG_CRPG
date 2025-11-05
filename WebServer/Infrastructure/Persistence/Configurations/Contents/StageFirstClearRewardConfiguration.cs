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
    public class StageFirstClearRewardConfiguration : IEntityTypeConfiguration<StageFirstClearReward>
    {
        public void Configure(EntityTypeBuilder<StageFirstClearReward> e) 
        {
            e.ToTable("StageFirstClearRewards");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

            e.Property(x => x.StageId).HasColumnName("stage_id").IsRequired();
            e.Property(x => x.ItemId).HasColumnName("item_id").IsRequired();
            e.Property(x => x.Qty).HasColumnName("qty").HasColumnType("smallint").IsRequired();

            e.HasIndex(x => x.StageId);
            e.HasIndex(x => x.ItemId);
            // 옵션: e.HasIndex(x => new { x.StageId, x.ItemId }).IsUnique();
        }
    }
}
