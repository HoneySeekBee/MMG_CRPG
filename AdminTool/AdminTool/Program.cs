using AdminTool.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AdminTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container

            builder.Services.AddHttpClient("GameApi", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["GameApiBaseUrl"]!);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<TokenAttachHandler>();
            builder.Services.AddScoped<AdminTool.Controllers.IStageUiProvider, AdminTool.Controllers.StaticStageUiProvider>();
            builder.Services.AddScoped<ICombatApiClient, CombatApiClient>();
            builder.Services.AddScoped<ICharacterUiProvider, ApiCharacterUiProvider>();
            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
.AddCookie(options =>
{
    options.LoginPath = "/admin/auth/login";
    options.LogoutPath = "/admin/auth/logout";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
            builder.Services.AddAuthorization();

            builder.Services.AddSession(o =>
            {
                o.Cookie.Name = ".AdminTool.Session";
                o.Cookie.HttpOnly = true; 
                o.Cookie.SecurePolicy = CookieSecurePolicy.None;
                o.IdleTimeout = TimeSpan.FromHours(2);
            });
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<TokenAttachHandler>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
