using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration cfg)
    {
        var connString = cfg.GetConnectionString("GameDb")!;
        var dsb = new NpgsqlDataSourceBuilder(connString);
        dsb.EnableDynamicJson();
        var dataSource = dsb.Build();

        services.AddDbContextPool<GameDBContext>(opt =>
            opt.UseNpgsql(dataSource, npg => npg.EnableRetryOnFailure())
               .EnableSensitiveDataLogging(false));

        services.AddPooledDbContextFactory<GameDBContext>(opt =>
            opt.UseNpgsql(dataSource, npg => npg.EnableRetryOnFailure())
               .EnableSensitiveDataLogging(false));

        return services;
    }
}
