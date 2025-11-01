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
    public class UserCharacterConfiguration : IEntityTypeConfiguration<UserCharacter>
    {
        public void Configure(EntityTypeBuilder<UserCharacter> e) 
        {
            e.ToTable("UserCharacters");               // 실제 테이블명과 일치
            e.HasKey(x => x.UserCharacterId);
            e.Property(x => x.UserCharacterId).HasColumnName("user_character_id");
            e.Property(x => x.UserId).HasColumnName("UserId");
            e.Property(x => x.CharacterId).HasColumnName("CharacterId");
            e.Property(x => x.Level).HasColumnName("Level");
            e.Property(x => x.Exp).HasColumnName("Exp");
            e.Property(x => x.BreakThrough).HasColumnName("BreakThrough");
            e.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt").IsRequired();
            e.Property(x => x.UpdatedAt).IsConcurrencyToken();

            var skillsNav = e.Metadata.FindNavigation(nameof(UserCharacter.Skills));
            skillsNav!.SetField("_skills");
            skillsNav.SetPropertyAccessMode(PropertyAccessMode.Field);

            e.HasMany(uc => uc.Skills)
    .WithOne(s => s.UserCharacter)
    .HasForeignKey(s => new { s.UserId, s.CharacterId })
    .HasPrincipalKey(uc => new { uc.UserId, uc.CharacterId })
    .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(uc => uc.Equips)
             .WithOne(eq => eq.UserCharacter)
             .HasForeignKey(eq => new { eq.UserId, eq.CharacterId })
             .HasPrincipalKey(uc => new { uc.UserId, uc.CharacterId })
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.CharacterId);
            e.HasIndex(x => x.UpdatedAt);
        }

    }
}
