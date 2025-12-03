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
                .HasColumnName("PoolId")
                .IsRequired();

            e.Property(x => x.CharacterId)
                .HasColumnName("CharacterId")
                .IsRequired();

            e.Property(x => x.Grade)
                .HasColumnName("Grade")
                .IsRequired();

            e.Property(x => x.RateUp)
                .HasColumnName("RateUp")
                .HasDefaultValue(false);

            e.Property(x => x.Weight)
                .HasColumnName("Weight")
                .IsRequired();

            e.HasCheckConstraint(
    "ck_gpe_weight_pos",
    "\"Weight\" > 0"
); 
        }
    }
}
