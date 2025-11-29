using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Gacha;

namespace Infrastructure.Persistence.Configurations.Gacha
{
    public sealed class GachaPoolEntryConfiguration : IEntityTypeConfiguration<GachaPoolEntry>
    {
        public void Configure(EntityTypeBuilder<GachaPoolEntry> e)
        {
            e.ToTable("GachaPoolEntry");

            // 복합 PK (PoolId + CharacterId)
            e.HasKey(x => new { x.PoolId, x.CharacterId });

            e.Property(x => x.PoolId)
                .HasColumnName("pool_id")
                .IsRequired();

            e.Property(x => x.CharacterId)
                .HasColumnName("character_id")
                .IsRequired();

            e.Property(x => x.Grade)
                .HasColumnName("grade")
                .IsRequired();

            e.Property(x => x.RateUp)
                .HasColumnName("rate_up")
                .HasDefaultValue(false);

            e.Property(x => x.Weight)
                .HasColumnName("weight")
                .IsRequired();

            // Check Constraint
            e.HasCheckConstraint(
                "ck_gpe_weight_pos",
                "\"weight\" > 0"
            );

            // 섀도우 FK 무시
            e.Ignore("GachaPoolPoolId");
        }
    }
}
