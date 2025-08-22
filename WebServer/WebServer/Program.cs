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
