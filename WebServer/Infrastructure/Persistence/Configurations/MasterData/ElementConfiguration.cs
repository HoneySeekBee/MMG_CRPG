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
    public class ElementConfiguration : IEntityTypeConfiguration<Element>
    {
        public void Configure(EntityTypeBuilder<Element> e)
        {
            e.ToTable("Element");
            e.HasKey(x => x.ElementId);
            e.Property(x => x.Key).IsRequired();
            e.Property(x => x.Label).IsRequired();
            e.Property(x => x.ColorHex).IsRequired();
            e.Property(x => x.SortOrder).HasColumnType("smallint");
            e.Property(x => x.Meta).HasColumnType("jsonb"); // PostgreSQL
            e.HasIndex(x => new { x.IsActive, x.SortOrder });
            e.HasIndex(x => x.Key).IsUnique();
        }
    }
}
