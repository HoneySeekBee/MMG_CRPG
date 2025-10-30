using Domain.Entities;
using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>,
        IEntityTypeConfiguration<UserCurrency>,
        IEntityTypeConfiguration<UserInventory>,
        IEntityTypeConfiguration<UserProfile>,
        IEntityTypeConfiguration<UserCharacter>,
        IEntityTypeConfiguration<UserCharacterEquip>,
        IEntityTypeConfiguration<UserCharacterSkill>,
        IEntityTypeConfiguration<UserParty>,
        IEntityTypeConfiguration<UserPartySlot>
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


        void IEntityTypeConfiguration<UserCurrency>.Configure(EntityTypeBuilder<UserCurrency> e)
        {
            e.ToTable("UserCurrency");
            e.HasKey(x => new { x.UserId, x.CurrencyId });
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.CurrencyId);
        }

        void IEntityTypeConfiguration<UserInventory>.Configure(EntityTypeBuilder<UserInventory> e)
        {
            e.ToTable("UserInventory");

            e.HasKey(x => new { x.Id });

            e.Property(x => x.Id).HasColumnName("Id");
            e.Property(x => x.UserId).HasColumnName("UserId");
            e.Property(x => x.ItemId).HasColumnName("ItemId");
            e.Property(x => x.Count).HasColumnName("Count");
            e.Property(x => x.UpdatedAt).IsRequired();

            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.ItemId);
            e.HasIndex(x => x.UpdatedAt);
        }
        void IEntityTypeConfiguration<UserProfile>.Configure(EntityTypeBuilder<UserProfile> e)
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
        void IEntityTypeConfiguration<UserCharacter>.Configure(EntityTypeBuilder<UserCharacter> e)
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
        void IEntityTypeConfiguration<UserCharacterEquip>.Configure(EntityTypeBuilder<UserCharacterEquip> e)
        {
            e.ToTable("UserCharacterEquip");

            e.HasKey(x => new { x.UserId, x.CharacterId, x.EquipId });

            e.Property(x => x.UserId).HasColumnName("UserId");
            e.Property(x => x.CharacterId).HasColumnName("CharacterId");
            e.Property(x => x.EquipId).HasColumnName("EquipId");
            e.Property(x => x.InventoryId).HasColumnName("InventoryId");

            e.HasOne(x => x.UserCharacter)
    .WithMany(uc => uc.Equips)
    .HasForeignKey(x => new { x.UserId, x.CharacterId })
    .HasPrincipalKey(uc => new { uc.UserId, uc.CharacterId })
    .OnDelete(DeleteBehavior.Cascade);


        }

        void IEntityTypeConfiguration<UserCharacterSkill>.Configure(EntityTypeBuilder<UserCharacterSkill> e)
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

        void IEntityTypeConfiguration<UserParty>.Configure(EntityTypeBuilder<UserParty> e)
        {  // table & keys
            e.ToTable("user_party");
            e.HasKey(x => x.PartyId);

            // columns
            e.Property(x => x.PartyId).HasColumnName("party_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.BattleId).HasColumnName("battle_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            // index for frequent lookup
            e.HasIndex(x => new { x.UserId, x.BattleId })
             .HasDatabaseName("ix_party_user_battle");

            // 관계: UserParty 1 ── * UserPartySlot
            e.HasMany<UserPartySlot>(nameof(UserParty.Slots))
             .WithOne()
             .HasForeignKey(s => s.PartyId)
             .HasPrincipalKey(p => p.PartyId)
             .OnDelete(DeleteBehavior.Cascade);

            // Slots는 읽기 전용 컬렉션(백킹필드 _slots) → 필드 접근
            e.Navigation(p => p.Slots)
             .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
        void IEntityTypeConfiguration<UserPartySlot>.Configure(EntityTypeBuilder<UserPartySlot> e)
        {
            e.ToTable("user_party_character");
            e.HasKey(x => new { x.PartyId, x.SlotId });

            // columns
            e.Property(x => x.PartyId).HasColumnName("party_id");
            e.Property(x => x.SlotId).HasColumnName("slot_id");
            e.Property(x => x.UserCharacterId).HasColumnName("user_character_id");

            // created_at / updated_at 는 도메인에 프로퍼티가 없으므로 Shadow Property로 매핑
            e.Property<DateTime>("created_at");
            e.Property<DateTime>("updated_at");

            // 같은 파티에서 같은 캐릭터 중복 금지 (NULL 제외 = 부분 유니크)
            e.HasIndex(x => new { x.PartyId, x.UserCharacterId })
             .HasDatabaseName("ux_upc_unique_char")
             .HasFilter("\"user_character_id\" IS NOT NULL")
             .IsUnique();
        }
    }
}
