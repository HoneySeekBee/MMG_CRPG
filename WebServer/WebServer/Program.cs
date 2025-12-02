using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebServer;
using WebServer.Extensions;
using StackExchange.Redis;
using Application.Common.Interface;
using Infrastructure.Services;
using WebServer.HostedServices;
using Amazon.S3;
using Application.Storage;
using WebServer.Options;
using ProtoBuf.Meta;
using WebServer.Filters;
using System.Data;
using WebServer.Seed;
using Npgsql;

public class Program
{
    public static async Task Main(string[] args)
    {
        var seedMode = args.Contains("load-seeds");
        var exportMode = args.Contains("export-seeds");

        var tempBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();


        var seedDir = Path.Combine(Directory.GetCurrentDirectory(), "DataSeeds");

        if (exportMode)
        {
            var cs = tempBuilder.GetConnectionString("LocalDevDb")
                     ?? tempBuilder.GetConnectionString("GameDb");

            if (string.IsNullOrWhiteSpace(cs))
            {
                Console.WriteLine("ERROR: Connection string LocalDevDb/GameDb not found.");
                return;
            }
            using var conn = new NpgsqlConnection(cs);
            await conn.OpenAsync();

            await new SeedExporter(conn, seedDir).ExportAllAsync();
            Console.WriteLine("Export done.");
            return;
        }


        if (seedMode)
        {
            var cs = tempBuilder.GetConnectionString("GameDb");
            using var conn = new NpgsqlConnection(cs);
            await conn.OpenAsync();
            await new SeedLoader(conn, seedDir).LoadAllAsync();
            Console.WriteLine("Seed load done.");
            return;
        }

        var builder = WebApplication.CreateBuilder(args);
         
        // 1) 옵션 + 기반
        builder.Services
            .AddPersistence(builder.Configuration)   // DbContext/Factory
            .AddProtoFormatters()                    // Protobuf 포맷터
            .AddJwtAuth(builder.Configuration)       // AuthN/AuthZ
            .AddApplicationServices(builder.Configuration) // DI 묶음
            .AddHostedWorkers();                     // 캐시 워밍업 등

        builder.Services.AddHealthChecks();
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var redisConn = cfg.GetValue<string>("Redis");
            return ConnectionMultiplexer.Connect(redisConn);
        });
        builder.Services.AddHostedService<HeartbeatService>(); 
        builder.Services.AddHostedService<GachaCacheWarmupService>(); 
        builder.Services.AddGrpcServices();
        builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonS3>();
        builder.Services.Configure<AssetsOptions>(
    builder.Configuration.GetSection("AssetsOptions"));


        builder.Services.AddSingleton<IIconStorage>(sp =>
    new S3IconStorage(
        sp.GetRequiredService<IAmazonS3>(),
        "mmg-crpg-korea-bucket-storage"
    ));
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<GameExceptionFilter>();
        });

        builder.Services.AddSingleton<IPortraitStorage>(sp =>
            new S3PortraitStorage(
                sp.GetRequiredService<IAmazonS3>(),
                "mmg-crpg-korea-bucket-storage"
            ));
        Environment.SetEnvironmentVariable("SERVER_ID", "web-1");

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

            var loader = new SeedLoader(db, seedDir);
            await loader.LoadAllAsync();
        }

        // 2) 미들웨어
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "RPG Backend API v1");
                c.RoutePrefix = "swagger";
            });
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        //app.UseHttpsRedirection();

        // 정적 파일 + 강 캐시 헤더(이미지)
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";
            }
        });
      

        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapRazorPages();

        app.MapGrpcServices();
        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapGet("/version", () => new
        {
            name = "RPG Backend",
            version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
            timeUtc = DateTime.UtcNow
        }).AllowAnonymous();

        app.MapGet("/redis-test", async (ICacheService cache) =>
        {
            await cache.SetAsync("ping:test", "HelloRedis!", TimeSpan.FromMinutes(1));
            var value = await cache.GetAsync("ping:test");
            return new { value };
        });

        app.Run();
    }
}
