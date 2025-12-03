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
    public sealed class GachaPoolConfiguration : IEntityTypeConfiguration<GachaPool>
    {
        public void Configure(EntityTypeBuilder<GachaPool> g)
        {
            g.ToTable("GachaPool");

            // PK
            g.HasKey(x => x.PoolId);

            g.Property(x => x.PoolId)
                .HasColumnName("PoolId")
                .ValueGeneratedOnAdd();

            g.Property(x => x.Name)
                .HasColumnName("Name")
                .IsRequired();

            g.Property(x => x.ScheduleStart)
                .HasColumnName("ScheduleStart")
                .HasColumnType("timestamptz")
                .IsRequired();

            g.Property(x => x.ScheduleEnd)
                .HasColumnName("ScheduleEnd")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            g.Property(x => x.PityJson)
                .HasColumnName("PityJson")
                .HasColumnType("jsonb");

            g.Property(x => x.Config)
                .HasColumnName("Config")
                .HasColumnType("jsonb");

            g.Property(x => x.TablesVersion)
                .HasColumnName("TablesVersion");

            // Entries 네비게이션(Readonly backing field)
            g.Metadata.FindNavigation(nameof(GachaPool.Entries))!
              .SetPropertyAccessMode(PropertyAccessMode.Field);

            // 관계
            g.HasMany(x => x.Entries)
             .WithOne()
             .HasForeignKey(x => x.PoolId)
             .IsRequired()
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
