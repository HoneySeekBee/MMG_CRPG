using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.MasterData
{
    public class RarityConfiguration : IEntityTypeConfiguration<Rarity>
    {
        public void Configure(EntityTypeBuilder<Rarity> e) 
        {
            e.ToTable("Rarity");
            e.HasKey(x => x.RarityId);
            e.Property(x => x.RarityId).ValueGeneratedOnAdd();
            e.Property(x => x.Stars).IsRequired();
            e.Property(x => x.Key).IsRequired();
            e.Property(x => x.Label).IsRequired();
            e.Property(x => x.ColorHex);
            e.Property(x => x.Meta).HasColumnType("jsonb");
            e.Property(x => x.IsActive).HasDefaultValue(true);

            e.HasIndex(x => x.Key).IsUnique();
        }
    }
}
