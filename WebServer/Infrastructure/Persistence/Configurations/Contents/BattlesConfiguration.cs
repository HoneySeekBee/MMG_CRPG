using Domain.Entities.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Contents
{
    public class BattlesConfiguration : IEntityTypeConfiguration<Battle>
    {
        public void Configure(EntityTypeBuilder<Battle> builder)
        {
            builder.ToTable("Battles");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                .HasColumnName("id");

            builder.Property(b => b.Name)
                .HasColumnName("name")
                .IsRequired();

            builder.Property(b => b.Active)
                .HasColumnName("active")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(b => b.SceneKey)
                .HasColumnName("scene_key")
                .IsRequired(false);

            builder.Property(b => b.CheckMulti)
                .HasColumnName("check_multi")
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(b => b.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(b => b.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("now()")
                .IsRequired();
        }
    }
}
