using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
// using Npgsql; // PostgreSQL
using System.Data;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("GameDb")!;
        var serverVersion = ServerVersion.AutoDetect(cs);

        // PostgreSQL
        // var dsb = new NpgsqlDataSourceBuilder(cs);
        // dsb.MapEnum<Domain.Enum.Characters.BodySize>("public.BodySize");
        // dsb.MapEnum<Domain.Enum.Characters.PartType>("public.PartType");
        // dsb.MapEnum<Domain.Enum.Characters.CharacterAnimationType>("public.CharacterAnimationType");
        // dsb.EnableDynamicJson();
        // var dataSource = dsb.Build();
        // services.AddSingleton(dataSource);

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
