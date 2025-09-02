using Application.Elements;
using Application.Factions;
using Application.Icons;
using Application.Rarities;
using Application.Repositories;
using Application.Roles;
using Application.SkillLevels;
using Application.Skills;
using Application.Storage;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace WebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddControllers();

            builder.Services.AddDbContextPool<GameDBContext>(opt =>
            opt.UseNpgsql(builder.Configuration.GetConnectionString("GameDb"),
            npg => npg.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null))
            .EnableSensitiveDataLogging(false));
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddScoped<IconService>();
            builder.Services.AddScoped<IIconRepository, EfIconRepository>();
            
            // 속성 및 속성 상속 
            builder.Services.AddScoped<IElementRepository, ElementRepository>();
            builder.Services.AddScoped<IElementService, ElementService>(); 
            builder.Services.AddScoped<Application.ElementAffinities.IElementAffinityService,
                            Application.ElementAffinities.ElementAffinityService>();
            builder.Services.AddScoped<Application.Repositories.IElementAffinityRepository,
                                        Infrastructure.Repositories.ElementAffinityRepository>();
            
            // 소속, 역할군, 희기도 
            builder.Services.AddScoped<IFactionRepository, FactionRepository>();   // Infra 구현체
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IRarityRepository, RarityRepository>();

            builder.Services.AddScoped<IFactionService, FactionService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IRarityService, RarityService>();

            // 스킬 관련 
            builder.Services.AddScoped<ISkillRepository, SkillRepository>();
            builder.Services.AddScoped<ISkillLevelRepository, SkillLevelRepository>();
            builder.Services.AddScoped<ISkillService, SkillService>();
            builder.Services.AddScoped<ISkillLevelService, SkillLevelService>();

            builder.Services.AddSingleton<IIconStorage>(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                var cfg = sp.GetRequiredService<IConfiguration>();

                return new LocalIconStorage(env.WebRootPath, cfg["PublicBaseUrl"]);
            });

            var conn = builder.Configuration.GetConnectionString("GameDb");
            var sb = new Npgsql.NpgsqlConnectionStringBuilder(conn);
            Console.WriteLine($"[ConnString] Host={sb.Host}; Port={sb.Port}; Database={sb.Database}; Username={sb.Username}; Password=***");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                    ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable"
            });
            app.MapControllers();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
