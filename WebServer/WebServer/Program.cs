using Application.Elements;
using Application.Icons;
using Application.Repositories;
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
            builder.Services.AddScoped<IElementRepository, ElementRepository>();
            builder.Services.AddScoped<IElementService, ElementService>(); 
            builder.Services.AddScoped<Application.ElementAffinities.IElementAffinityService,
                            Application.ElementAffinities.ElementAffinityService>();
            builder.Services.AddScoped<Application.Repositories.IElementAffinityRepository,
                                        Infrastructure.Repositories.ElementAffinityRepository>();


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
