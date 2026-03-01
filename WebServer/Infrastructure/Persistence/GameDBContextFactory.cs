using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence
{
    public class GameDBContextFactory : IDesignTimeDbContextFactory<GameDBContext>
    {
        public GameDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GameDBContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=mmg_crpg;Username=postgres;Password=postgres");
            return new GameDBContext(optionsBuilder.Options);
        }
    }
}
