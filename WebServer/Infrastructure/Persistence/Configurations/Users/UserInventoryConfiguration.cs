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
    public class UserInventoryConfiguration : IEntityTypeConfiguration<UserInventory>
    {
        public void Configure(EntityTypeBuilder<UserInventory> e)
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

    }
}
