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
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> e) 
        {
            e.ToTable("UsersProfiles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ProfileId").ValueGeneratedOnAdd();

            e.Property(x => x.UserId).IsRequired();
            e.HasIndex(x => x.UserId).IsUnique();

            e.Property(x => x.NickName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Level).IsRequired();
            e.Property(x => x.Exp).IsRequired();

            e.Property(x => x.Gold).IsRequired();
            e.Property(x => x.Gem).IsRequired();
            e.Property(x => x.Token).IsRequired();

            e.Property(x => x.IconId).IsRequired(false);
        }
    }
}
