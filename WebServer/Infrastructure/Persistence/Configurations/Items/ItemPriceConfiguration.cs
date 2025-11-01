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
    public class ItemPriceConfiguration : IEntityTypeConfiguration<ItemPrice>
    {
        public void Configure(EntityTypeBuilder<ItemPrice> e) 
        {
            e.ToTable("ItemPrice");
            e.HasKey(x => x.Id);
            e.Property(x => x.Price).HasColumnType("bigint");
            e.HasIndex(x => new { x.ItemId, x.CurrencyId, x.PriceType }).IsUnique();
        }
    }
}
