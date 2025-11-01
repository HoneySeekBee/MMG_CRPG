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
    public class UserCharacterEquipConfiguration : IEntityTypeConfiguration<UserCharacterEquip>
    {
        public void Configure(EntityTypeBuilder<UserCharacterEquip> e) 
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
    }
}
