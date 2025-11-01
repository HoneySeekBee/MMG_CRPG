using Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Characters
{
    public class CharacterModelPartConfiguration : IEntityTypeConfiguration<CharacterModelPart>
    {
        public void Configure(EntityTypeBuilder<CharacterModelPart> e)
        {
            e.ToTable("CharacterModelParts");
            e.HasKey(x => x.PartId);

            e.Property(x => x.PartId).HasColumnName("part_id");
            e.Property(x => x.PartKey).HasColumnName("part_key").IsRequired();

            e.Property(x => x.PartType)
                .HasColumnName("part_type")
                .HasColumnType("PartType");

            e.HasIndex(x => x.PartKey).IsUnique().HasDatabaseName("ux_part_key");
            e.HasIndex(x => new { x.PartType }).HasDatabaseName("ix_parts_type_size");
        }
    }
}
