using Domain.Entities;
using Domain.Entities.Gacha;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Gacha
{
    public sealed class GachaBannerConfiguration : IEntityTypeConfiguration<GachaBanner>
    {
        public void Configure(EntityTypeBuilder<GachaBanner> builder)
        {
            builder.ToTable("GachaBanner");

            // PK
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            // Unique Key (운영툴용 Key)
            builder.Property(x => x.Key)
                .HasColumnName("key")
                .IsRequired();

            builder.HasIndex(x => x.Key).IsUnique();

            // Title / Subtitle
            builder.Property(x => x.Title)
                .HasColumnName("title")
                .IsRequired();

            builder.Property(x => x.Subtitle)
                .HasColumnName("subtitle");

            // Portrait Image (nullable)
            builder.Property(x => x.PortraitId)
                .HasColumnName("portrait_id");

            // Gacha Pool 연결
            builder.Property(x => x.GachaPoolId)
                .HasColumnName("gacha_pool_id")
                .IsRequired();

            // Schedule
            builder.Property(x => x.StartsAt)
                .HasColumnName("starts_at")
                .HasColumnType("timestamptz")
                .IsRequired();

            builder.Property(x => x.EndsAt)
                .HasColumnName("ends_at")
                .HasColumnType("timestamptz");

            // Priority
            builder.Property(x => x.Priority)
                .HasColumnName("priority")
                .IsRequired();

            // Status (enum → smallint)
            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion<short>()
                .IsRequired();

            // IsActive
            builder.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            builder.Property<int>("CostCurrencyId")
                .HasColumnName("cost_currency_id")
                .IsRequired(false); // 티켓-only 배너 가능

            builder.Property<int>("Cost")
                .HasColumnName("cost")
                .IsRequired(false);

            builder.Property<int>("TicketItemId")
                .HasColumnName("ticket_item_id")
                .IsRequired(false);

            // 외래키 구성 (Currency, Item은 선택적)
            builder.HasOne<Currency>()
                .WithMany()
                .HasForeignKey("CostCurrencyId")
                .HasPrincipalKey(c => c.Id)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Item>()
                .WithMany()
                .HasForeignKey("TicketItemId")
                .HasPrincipalKey(i => i.Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
