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
                .HasColumnName("pool_id")
                .ValueGeneratedOnAdd();

            g.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired();

            g.Property(x => x.ScheduleStart)
                .HasColumnName("schedule_start")
                .HasColumnType("timestamptz")
                .IsRequired();

            g.Property(x => x.ScheduleEnd)
                .HasColumnName("schedule_end")
                .HasColumnType("timestamptz")
                .IsRequired(false);

            g.Property(x => x.PityJson)
                .HasColumnName("pity_json")
                .HasColumnType("jsonb");

            g.Property(x => x.Config)
                .HasColumnName("config_json")
                .HasColumnType("jsonb");

            g.Property(x => x.TablesVersion)
                .HasColumnName("tables_version");

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
