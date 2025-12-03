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
                .HasColumnName("BannerId");  // DB 그대로

            // Key
            builder.Property(x => x.Key)
                .HasColumnName("Key")
                .IsRequired();
            builder.HasIndex(x => x.Key).IsUnique();

            // Title
            builder.Property(x => x.Title)
                .HasColumnName("Title")
                .IsRequired();

            // Subtitle
            builder.Property(x => x.Subtitle)
                .HasColumnName("Subtitle");

            // PortraitId
            builder.Property(x => x.PortraitId)
                .HasColumnName("PortraitId");

            // GachaPoolId
            builder.Property(x => x.GachaPoolId)
                .HasColumnName("GachaPoolId")
                .IsRequired();

            // StartsAt
            builder.Property(x => x.StartsAt)
                .HasColumnName("StartsAt")
                .HasColumnType("timestamptz")
                .IsRequired();

            // EndsAt
            builder.Property(x => x.EndsAt)
                .HasColumnName("EndsAt")
                .HasColumnType("timestamptz");

            // Priority
            builder.Property(x => x.Priority)
                .HasColumnName("Priority")
                .IsRequired();

            // Status
            builder.Property(x => x.Status)
                .HasColumnName("Status")
                .HasConversion<short>()
                .IsRequired();

            // IsActive
            builder.Property(x => x.IsActive)
                .HasColumnName("IsActive")
                .IsRequired();

            // cost_currency_id (snake_case)
            builder.Property(x => x.CostCurrencyId)
                .HasColumnName("cost_currency_id")
                .IsRequired();

            // ticket_item_id
            builder.Property(x => x.TicketItemId)
                .HasColumnName("ticket_item_id")
                .IsRequired();

            // cost
            builder.Property(x => x.Cost)
                .HasColumnName("cost")
                .IsRequired();

            // Relations
            builder.HasOne<Currency>()
                .WithMany()
                .HasForeignKey(x => x.CostCurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Item>()
                .WithMany()
                .HasForeignKey(x => x.TicketItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
