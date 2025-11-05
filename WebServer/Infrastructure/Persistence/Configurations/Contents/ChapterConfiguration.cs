using Domain.Entities.Contents;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Contents
{
    public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
    {
        public void Configure(EntityTypeBuilder<Chapter> builder)
        {
            builder.ToTable("Chapters");

            builder.HasKey(c => c.ChapterId);

            builder.Property(c => c.ChapterId)
                .HasColumnName("chapter_id");

            builder.Property(c => c.BattleId)
                .HasColumnName("battle_id")
                .IsRequired();

            builder.Property(c => c.ChapterNum)
                .HasColumnName("chapter_num")
                .IsRequired();

            builder.Property(c => c.Name)
                .HasColumnName("name")
                .IsRequired();

            builder.Property(c => c.Description)
                .HasColumnName("description");

            builder.Property(c => c.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("now()")
                .IsRequired();
        }
    }
}
