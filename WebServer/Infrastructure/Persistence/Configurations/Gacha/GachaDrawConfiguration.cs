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
    public sealed class GachaDrawLogConfiguration : IEntityTypeConfiguration<GachaDrawLog>
    {
        public void Configure(EntityTypeBuilder<GachaDrawLog> builder)
        {
            builder.ToTable("GachaDrawLog");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.BannerId)
                .HasColumnName("banner_id")
                .IsRequired();

            builder.Property(x => x.PoolId)
                .HasColumnName("pool_id")
                .IsRequired();

            builder.Property(x => x.Timestamp)
                .HasColumnName("time_stamp")
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(x => x.ResultsJson)
                .HasColumnName("results_json")
                .HasColumnType("jsonb")
                .IsRequired();
        }
    }
}
