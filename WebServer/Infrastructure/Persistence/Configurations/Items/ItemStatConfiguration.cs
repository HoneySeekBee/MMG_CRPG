using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Items
{
    public class ItemStatConfiguration : IEntityTypeConfiguration<ItemStat>
    {
        public void Configure(EntityTypeBuilder<ItemStat> e) 
        {
            e.ToTable("ItemStat");
            e.HasKey(x => x.Id);
            e.Property(x => x.Value).HasColumnType("numeric(12,4)");
            e.HasIndex(x => new { x.ItemId, x.StatId }).IsUnique();

            e.HasOne(x => x.StatType)
                .WithMany()
                .HasForeignKey(x => x.StatId)
                .HasPrincipalKey(t => t.Id);
        }
    }
}
