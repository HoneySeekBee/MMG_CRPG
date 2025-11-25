using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebServer;
using WebServer.Extensions;  
using StackExchange.Redis;
using Application.Common.Interface;
using Infrastructure.Services;
using WebServer.HostedServices;

public class Program
{
    public static void Main(string[] args)
    {
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
            return ConnectionMultiplexer.Connect("localhost:6379");
        });
        builder.Services.AddHostedService<HeartbeatService>();

        builder.Services.AddGrpcServices();

        Environment.SetEnvironmentVariable("SERVER_ID", "web-1");

        var app = builder.Build();

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

        app.UseHttpsRedirection();

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
