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
    public class UserCharacterSkillConfiguration : IEntityTypeConfiguration<UserCharacterSkill>
    {
        public void Configure(EntityTypeBuilder<UserCharacterSkill> e) 
        {
            e.ToTable("UserCharacterSkill");          // 실제 테이블명 (복수 추천)
            e.HasKey(x => new { x.UserId, x.CharacterId, x.SkillId });

            e.Property(x => x.UserId).HasColumnName("UserId");
            e.Property(x => x.CharacterId).HasColumnName("CharacterId");
            e.Property(x => x.SkillId).HasColumnName("SkillId");
            e.Property(x => x.Level).HasColumnName("Level");
            e.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt").IsRequired();
            e.Property(x => x.UpdatedAt).IsConcurrencyToken();

            //  
            e.HasOne(s => s.UserCharacter)
.WithMany(uc => uc.Skills)
.HasForeignKey(s => new { s.UserId, s.CharacterId })
.HasPrincipalKey(uc => new { uc.UserId, uc.CharacterId })
.OnDelete(DeleteBehavior.Cascade);

            // (선택) 인덱스
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.CharacterId);
            e.HasIndex(x => x.SkillId);
            e.HasIndex(x => x.UpdatedAt);
        }
    }
}
