using Application.Character;
using Application.Combat;
using Application.Currency;
using Application.Elements;
using Application.Factions;
using Application.GachaBanner;
using Application.GachaPool;
using Application.Icons;
using Application.Items;
using Application.ItemTypes;
using Application.Portraits;
using Application.Rarities;
using Application.Repositories;
using Application.Roles;
using Application.SkillLevels;
using Application.Skills;
using Application.Storage;
using Domain.Services;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

            var connString = builder.Configuration.GetConnectionString("GameDb");

            var dsb = new NpgsqlDataSourceBuilder(connString);
            dsb.EnableDynamicJson();
            var dataSource = dsb.Build();

            builder.Services.AddDbContextPool<GameDBContext>(opt =>
    opt.UseNpgsql(dataSource, npg =>
        npg.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null))
       .EnableSensitiveDataLogging(false)
);

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddScoped<IconService>();

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

            // 캐릭터 관련 
            builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
            builder.Services.AddScoped<ICharacterService, CharacterService>();

            builder.Services.AddScoped<IPortraitRepository, EfPortraitRepository>();
            builder.Services.AddScoped<IPortraitStorage>(sp =>
                new LocalPortraitStorage(sp.GetRequiredService<IWebHostEnvironment>().WebRootPath!, builder.Configuration["PublicBaseUrl"]!));
            builder.Services.AddScoped<PortraitService>();

            // 아이템 관련 

            builder.Services.AddScoped<IItemRepository, ItemRepository>();
            builder.Services.AddScoped<IItemService, ItemService>();


            builder.Services.AddScoped<ICurrencyRepository, EFCurrencyRepository>();
            builder.Services.AddScoped<ICurrencyService, CurrencyService>();

            builder.Services.AddScoped<IItemTypeService, ItemTypeService>();
            builder.Services.AddScoped<IItemTypeRepository, EFItemTypeRepository>();

            // 전투 관련 
            builder.Services.AddScoped<ICombatService, CombatService>();
            //builder.Services.AddScoped<IMasterDataProvider, MasterDataProvider>();
            builder.Services.AddScoped<IMasterDataProvider, FakeMasterDataProvider>();

            builder.Services.AddSingleton<ICombatEngine, SimpleCombatEngine>();
            builder.Services.AddScoped<ICombatRepository, EfCombatRepository>();

            // 스텟 관련
            builder.Services.AddScoped<Application.StatTypes.IStatTypeService, Application.StatTypes.StatTypeService>();
            builder.Services.AddScoped<Application.Repositories.IStatTypeRepository, Infrastructure.Repositories.EFStatTypeRepository>();

            // 뽑기 관련
            builder.Services.AddScoped<IGachaBannerRepository, GachaBannerRepository>();
            builder.Services.AddScoped<IGachaBannerService, GachaBannerService>();

            builder.Services.AddScoped<IGachaPoolRepository, GachaPoolRepository>();
            builder.Services.AddScoped<IGachaPoolService, GachaPoolService>();

            builder.Services.AddScoped<IIconRepository, EfIconRepository>();
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
