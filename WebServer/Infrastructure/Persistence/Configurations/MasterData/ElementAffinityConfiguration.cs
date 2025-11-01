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
    public class ElementAffinityConfiguration : IEntityTypeConfiguration<ElementAffinity>
    {
        public void Configure(EntityTypeBuilder<ElementAffinity> e) 
        {
            e.ToTable("ElementAffinity");
            e.HasKey(x => new { x.AttackerElementId, x.DefenderElementId });
            e.Property(x => x.Multiplier)
            .HasColumnType("numeric(4,2)")
            .HasDefaultValue(1.00m);
        }
    }
}
