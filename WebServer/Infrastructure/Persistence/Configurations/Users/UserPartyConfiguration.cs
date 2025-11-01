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
    public class UserPartyConfiguration : IEntityTypeConfiguration<UserParty>
    {
        public void Configure(EntityTypeBuilder<UserParty> e) 
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
    }
}
