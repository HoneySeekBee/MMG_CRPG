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
    public class UserPartySlotConfiguration : IEntityTypeConfiguration<UserPartySlot>
    {
        public void Configure(EntityTypeBuilder<UserPartySlot> e) 
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
