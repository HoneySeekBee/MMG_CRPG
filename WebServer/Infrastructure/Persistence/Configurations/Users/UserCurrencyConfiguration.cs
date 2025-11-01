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
    public class UserCurrencyConfiguration : IEntityTypeConfiguration<UserCurrency>
    {
        public void Configure(EntityTypeBuilder<UserCurrency> e) 
        {
            e.ToTable("UserCurrency");
            e.HasKey(x => new { x.UserId, x.CurrencyId });
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
            e.HasOne<Currency>().WithMany().HasForeignKey(x => x.CurrencyId);
        }

    }
}
