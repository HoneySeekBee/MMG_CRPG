using Domain.Entities.User;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Contents;

namespace Infrastructure.Persistence.Configurations.Users
{
    internal class UserStageProgressConfiguration : IEntityTypeConfiguration<UserStageProgress>
    {
        public void Configure(EntityTypeBuilder<UserStageProgress> e)
        {
            e.ToTable("UserStageProgresses");
            e.HasKey(x => new { x.UserId, x.StageId });

            e.Property(x => x.UserId).HasColumnName("UserId").IsRequired();
            e.Property(x => x.StageId).HasColumnName("StageId").IsRequired();

            e.Property(x => x.Cleared)
             .HasColumnName("Cleared")
             .HasDefaultValue(false)
             .IsRequired();

            e.Property(x => x.Stars)
             .HasColumnName("Stars")
             .HasColumnType("smallint")
             .IsRequired();

            e.Property(x => x.ClearedAt)
             .HasColumnName("ClearedAt")
             .IsRequired(false);

            e.HasIndex(x => x.StageId);

            e.HasOne<Stage>()
             .WithMany()
             .HasForeignKey(x => x.StageId)
             .OnDelete(DeleteBehavior.Restrict); // 유저 데이터 보호

            e.HasOne<User>()
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
