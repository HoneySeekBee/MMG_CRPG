using Domain.Entities.Characters;
using Domain.Entities.Skill;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations.Characters
{
    internal class CharacterSkillConfiguration : IEntityTypeConfiguration<CharacterSkill>
    {
        public void Configure(EntityTypeBuilder<CharacterSkill> e)
        {
            e.ToTable("CharacterSkills");

            e.HasKey(x => new { x.CharacterId, x.Slot });

            e.Property(x => x.CharacterId).HasColumnName("CharacterId").IsRequired();
            e.Property(x => x.Slot).HasConversion<short>().IsRequired();
            e.Property(x => x.SkillId).IsRequired();
            e.Property(x => x.UnlockTier).HasDefaultValue(0);
            e.Property(x => x.UnlockLevel).HasDefaultValue(0);

            // (선택) 캐릭터 내 동일 스킬 중복 방지
            // e.HasAlternateKey(x => new { x.CharacterId, x.SkillId });

            e.HasOne(x => x.Character)
             .WithMany(c => c.CharacterSkills)            // ← Character 엔티티에 컬렉션 네비게이션 선언 필요
             .HasForeignKey(x => x.CharacterId)
             .HasPrincipalKey(c => c.Id)                  // ← 주키를 명확히
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne<Skill>()
             .WithMany()
             .HasForeignKey(x => x.SkillId)
             .OnDelete(DeleteBehavior.Restrict);

            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_cs_unlock_tier", "\"UnlockTier\" >= 0");
                t.HasCheckConstraint("ck_cs_unlock_level", "\"UnlockLevel\" >= 1");
                t.HasCheckConstraint("ck_cs_slot", "\"Slot\" BETWEEN 1 AND 4");
            });

            e.HasIndex(x => x.SkillId);
        }
    }
}
