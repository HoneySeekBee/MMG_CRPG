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
using Application.Synergy;
using Application.Users;
using Domain.Services;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Storage;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Security.Claims;
using System.Text;
using Application.Stages;
using WebServer.Formatters;
using Application.UserCurrency;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

namespace WebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
            var jwtIssuer = builder.Configuration["Jwt:Issuer"]; 
            var jwtAudience = builder.Configuration["Jwt:Audience"];
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
            builder.Services.AddControllers(opts =>
            {
                // Google.Protobuf(IMessage)용 포매터를 최우선으로 꽂음
                opts.InputFormatters.Insert(0, new ProtobufInputFormatter());
                opts.OutputFormatters.Insert(0, new ProtobufOutputFormatter());
            });

            builder.Services
       .AddAuthentication(options =>
       {
           options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
           options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
       })
       .AddJwtBearer(options =>
       {
           options.RequireHttpsMetadata = true;
           options.SaveToken = false;
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuerSigningKey = true,
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
               ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
               ValidIssuer = jwtIssuer,
               ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
               ValidAudience = jwtAudience,
               ValidateLifetime = true,
               ClockSkew = TimeSpan.Zero,
               NameClaimType = ClaimTypes.NameIdentifier
           };
       });

            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo { Title = "RPG Backend API", Version = "v1" });

                // JWT Authorize 버튼
                var scheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Bearer 토큰 (예: Bearer eyJ...)"
                };
                o.AddSecurityDefinition("Bearer", scheme);
                o.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });

                // 충돌 방지: 클래스 전체이름(네임스페이스 포함)으로 스키마 ID 생성
                o.CustomSchemaIds(t => t.FullName?.Replace("+", ".")); // nested type 대비
            });
            builder.Services.AddHealthChecks();
            builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));


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

            builder.Services.AddScoped<IUserCurrencyRepository, UserCurrencyRepository>();
            builder.Services.AddScoped<IWalletService, WalletService>();
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

            // 스테이지 관련
            builder.Services.AddScoped<IStagesRepository, EfStagesRepository>();
            builder.Services.AddScoped<IStageQueryRepository, EfStageQueryRepository>();
            builder.Services.AddScoped<IStagesService, StagesService>();

            builder.Services.AddScoped<ISecurityEventSink, SecurityEventSink>();

            // 시너지 관련 
            builder.Services.AddScoped<ISynergyRepository, SynergyRepository>();
            builder.Services.AddScoped<ISynergyService, SynergyService>();

            // 유저 관련 
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddSingleton<IClock, SystemClock>();
            builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
            builder.Services.AddSingleton<ITokenService>(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var key = cfg["Jwt:Key"]!;
                var issuer = cfg["Jwt:Issuer"];
                var audience = cfg["Jwt:Audience"];

                return new JwtTokenService(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    issuer,
                    audience,
                    accessTtl: TimeSpan.FromMinutes(30),
                    refreshTtl: TimeSpan.FromDays(7));
            });
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserQueryRepository, UserQueryRepository>();
            builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
            builder.Services.AddScoped<ISessionRepository, SessionRepository>();
            builder.Services.AddScoped<ISessionQueryRepository, SessionQueryRepository>();
            builder.Services.AddScoped<ISecurityEventRepository, SecurityEventRepository>();


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
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RPG Backend API v1");
                    c.RoutePrefix = "swagger"; // /swagger 로 접근
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapRazorPages();

            app.MapHealthChecks("/health", new HealthCheckOptions()).AllowAnonymous();

            app.MapGet("/version", () => new
            {
                name = "RPG Backend",
                version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
                timeUtc = DateTime.UtcNow
            }).AllowAnonymous();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<GameDBContext>();
                SeedData.EnsureSeeded(db);
            }
            app.Run();
        }
    }
}
