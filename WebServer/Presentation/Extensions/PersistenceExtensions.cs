using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System.Data;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("GameDb")!;
        var serverVersion = ServerVersion.AutoDetect(cs);

        // MySQL 설정
        services.AddDbContextFactory<GameDBContext>((sp, opt) =>
        {
            Console.WriteLine($"[Startup] Using MySQL: {serverVersion}");
            opt.UseMySql(cs, serverVersion);
        });

        services.AddScoped<IDbConnection>(sp =>
        { 
            return new MySqlConnection(cs);
        });

        return services;
    }
}
