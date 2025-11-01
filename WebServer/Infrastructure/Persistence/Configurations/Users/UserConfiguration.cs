using Domain.Entities;
using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Users
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>       
    {
        public void Configure(EntityTypeBuilder<User> e)
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();

            e.Property(x => x.Account).HasColumnName("Account").HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.Account).IsUnique();

            e.Property(x => x.PasswordHash).HasColumnName("PasswordHash").IsRequired();
            e.Property(x => x.Status).HasColumnName("Status").HasConversion<short>();

            e.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
            e.Property(x => x.LastLoginAt)
.HasColumnName("LastLoginAt")
.IsRequired(false);

            // 1:1 Profile (FK: UserProfile.UserId)
            e.HasOne(x => x.Profile)
             .WithOne()
             .HasForeignKey<UserProfile>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        }


     
    }
}
