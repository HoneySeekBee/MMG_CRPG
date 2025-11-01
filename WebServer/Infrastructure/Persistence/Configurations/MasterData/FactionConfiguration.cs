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
    public class FactionConfiguration : IEntityTypeConfiguration<Faction>
    {
        public void Configure(EntityTypeBuilder<Faction> e) 
        {
            e.ToTable("Faction");
            e.HasKey(x => x.FactionId);
            e.Property(x => x.FactionId).ValueGeneratedOnAdd();
            e.Property(x => x.Key).IsRequired();
            e.Property(x => x.Label).IsRequired();
            e.Property(x => x.ColorHex);
            e.Property(x => x.Meta).HasColumnType("jsonb");      // pg jsonb
            e.Property(x => x.IsActive).HasDefaultValue(true);

            e.HasIndex(x => x.Key).IsUnique();
        }
    }
}
