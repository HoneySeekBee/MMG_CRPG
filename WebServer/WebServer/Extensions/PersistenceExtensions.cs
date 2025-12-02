using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration cfg)
    {
        var cs = cfg.GetConnectionString("GameDb")!;

        // (A) DataSource 구성 (enum/json/opt-in)

        var dsb = new NpgsqlDataSourceBuilder(cs);
        dsb.MapEnum<Domain.Enum.Characters.BodySize>("public.BodySize");
        dsb.MapEnum<Domain.Enum.Characters.PartType>("public.PartType");
        dsb.MapEnum<Domain.Enum.Characters.CharacterAnimationType>("public.CharacterAnimationType");
        dsb.EnableDynamicJson();
        //dsb.EnableUnmappedTypes(); // 디버깅 혼란만 줌. 끄는 걸 권장.

        var dataSource = dsb.Build();
        services.AddSingleton(dataSource);

        // (C) DbContextFactory가 반드시 그 DataSource를 쓰도록 연결
        services.AddDbContextFactory<GameDBContext>((sp, opt) =>
        {
            var ds = sp.GetRequiredService<NpgsqlDataSource>();
            Console.WriteLine($"[Startup] DataSource hash = {ds.GetHashCode()}");
            opt.UseNpgsql(ds);
        });
        services.AddScoped<IDbConnection>(sp =>
        {
            var ds = sp.GetRequiredService<NpgsqlDataSource>();
            // Dapper가 사용할 새로운 커넥션 반환
            return ds.CreateConnection();
        });

        return services;
    }
}
