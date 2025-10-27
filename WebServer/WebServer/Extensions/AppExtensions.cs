using Application.Items;
using Application.ItemTypes;
using Application.Users;
using Application.Stages;
using Application.UserInventory;
using Application.UserCharacter;
using Application.UserCurrency;
using Infrastructure.Caching;
using Infrastructure.Repositories;
using Infrastructure.Storage;
using Domain.Services;
using Application.Icons;
using Application.Portraits;
using Application.Repositories;
using Application.Storage;
using Infrastructure.Auth;
using System.Text;
using WebServer.Options;
using Application.Character;
using Application.Combat;
using Application.Currency;
using Application.Elements;
using Application.Factions;
using Application.GachaBanner;
using Application.GachaPool;
using Application.Rarities;
using Application.Roles;
using Application.SkillLevels;
using Application.Skills;
using Application.Synergy;
using Microsoft.OpenApi.Models;
using Application.EquipSlots;
using Application.UserCharacterEquips;

namespace WebServer.Extensions
{
    public static class AppExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection s, IConfiguration cfg)
        {
            // 옵션 바인딩
            s.Configure<AssetsOptions>(cfg);

            // 공통
            s.AddRazorPages();
            s.AddControllers();
            s.AddEndpointsApiExplorer();
            s.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo { Title = "RPG Backend API", Version = "v1" });
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
                o.CustomSchemaIds(t => t.FullName?.Replace("+", "."));
            });

            s.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

            // 캐시 (싱글턴)
            s.AddSingleton<IItemTypeCache, ItemTypeCache>();
            s.AddSingleton<IIconCache, IconCache>();
            s.AddSingleton<IPortraitsCache, PortraitCache>();
            s.AddSingleton<IItemCache, ItemCache>();
            s.AddSingleton<IRarityCache, RarityCache>();
            s.AddSingleton<IElementCache, ElementCache>();
            s.AddSingleton<IRoleCache, RoleCache>();
            s.AddSingleton<IFactionCache, FactionCache>();
            s.AddSingleton<ISkillCache, SkillCache>();
            s.AddSingleton<ICharacterCache, CharacterCache>();
            s.AddSingleton<ICharacterExpCache, CharacterExpCache>();
            s.AddSingleton<IEquipSlotCache, EquipSlotCache>();

            // 도메인/리포지토리/서비스 (Scoped)

            // 아이콘/초상화
            s.AddScoped<IconService>();
            s.AddScoped<IIconRepository, EfIconRepository>();
            s.AddScoped<IPortraitRepository, EfPortraitRepository>();
            s.AddScoped<IPortraitStorage>(sp =>
                new LocalPortraitStorage(
                    sp.GetRequiredService<IWebHostEnvironment>().WebRootPath!,
                    cfg["PublicBaseUrl"]!));
            s.AddScoped<PortraitService>();

            s.AddSingleton<IIconStorage>(sp =>
            {
                var env = sp.GetRequiredService<IWebHostEnvironment>();
                return new LocalIconStorage(env.WebRootPath, cfg["PublicBaseUrl"]);
            });

            // 속성/속성상속(원소)
            s.AddScoped<IElementRepository, ElementRepository>();
            s.AddScoped<IElementService, ElementService>();
            s.AddScoped<Application.ElementAffinities.IElementAffinityService,
                        Application.ElementAffinities.ElementAffinityService>();
            s.AddScoped<Application.Repositories.IElementAffinityRepository,
                        Infrastructure.Repositories.ElementAffinityRepository>();

            // 소속 / 역할군 / 희귀도
            s.AddScoped<IFactionRepository, FactionRepository>();
            s.AddScoped<IRoleRepository, RoleRepository>();
            s.AddScoped<IRarityRepository, RarityRepository>();
            s.AddScoped<IFactionService, FactionService>();
            s.AddScoped<IRoleService, RoleService>();
            s.AddScoped<IRarityService, RarityService>();

            // 스킬
            s.AddScoped<ISkillRepository, SkillRepository>();
            s.AddScoped<ISkillService, SkillService>();
            s.AddScoped<ISkillLevelRepository, SkillLevelRepository>();
            s.AddScoped<ISkillLevelService, SkillLevelService>();


            // 캐릭터
            s.AddScoped<ICharacterRepository, CharacterRepository>();
            s.AddScoped<ICharacterService, CharacterService>();

            // 아이템 / 아이템 타입
            s.AddScoped<IItemRepository, ItemRepository>();
            s.AddScoped<IItemService, ItemService>();
            s.AddScoped<IItemTypeRepository, EFItemTypeRepository>();
            s.AddScoped<IItemTypeService, ItemTypeService>();
            s.AddScoped<IEquipSlotsRepository, EquipSlotsRepository>();
            s.AddScoped<IEquipSlotsService, EquipSlotsService>();

            // 통화/지갑
            s.AddScoped<IUserCurrencyRepository, UserCurrencyRepository>();
            s.AddScoped<IWalletService, WalletService>();
            s.AddScoped<ICurrencyRepository, EFCurrencyRepository>();
            s.AddScoped<ICurrencyService, CurrencyService>();

            // 전투
            s.AddScoped<ICombatService, CombatService>();
            s.AddScoped<ICombatRepository, EfCombatRepository>();
            s.AddScoped<IMasterDataProvider, FakeMasterDataProvider>();

            // 전투 엔진
            s.AddSingleton<ICombatEngine, SimpleCombatEngine>();

            // 스탯 타입
            s.AddScoped<Application.StatTypes.IStatTypeService, Application.StatTypes.StatTypeService>();
            s.AddScoped<Application.Repositories.IStatTypeRepository, Infrastructure.Repositories.EFStatTypeRepository>();

            // 가챠
            s.AddScoped<IGachaBannerRepository, GachaBannerRepository>();
            s.AddScoped<IGachaBannerService, GachaBannerService>();
            s.AddScoped<IGachaPoolRepository, GachaPoolRepository>();
            s.AddScoped<IGachaPoolService, GachaPoolService>();

            // 스테이지
            s.AddScoped<IStagesRepository, EfStagesRepository>();
            s.AddScoped<IStageQueryRepository, EfStageQueryRepository>();
            s.AddScoped<IStagesService, StagesService>();

            // 시너지
            s.AddScoped<ISynergyRepository, SynergyRepository>();
            s.AddScoped<ISynergyService, SynergyService>();

            // 유저
            s.AddScoped<IUserService, UserService>();
            s.AddScoped<IUserRepository, UserRepository>();
            s.AddScoped<IUserQueryRepository, UserQueryRepository>();
            s.AddScoped<IProfileRepository, ProfileRepository>();
            s.AddScoped<ISessionRepository, SessionRepository>();
            s.AddScoped<ISessionQueryRepository, SessionQueryRepository>();
            s.AddScoped<ISecurityEventRepository, SecurityEventRepository>();
            s.AddScoped<IUserCharacterEquipRepository, UserCharacterEquipRepository>();

            // 유저 인벤토리
            s.AddScoped<IUserInventoryRepository, UserInventoryRepository>();
            s.AddScoped<IUserInventoryQueryRepository, UserInventoryQueryRepository>();
            s.AddScoped<IUserInventoryService, UserInventoryService>();

            // 유저 캐릭터
            s.AddScoped<IUserCharacterRepository, UserCharacterRepository>();
            s.AddScoped<IUserCharacterService, UserCharacterService>();
            s.AddScoped<ICharacterEquipmentService, CharacterEquipmentService>();

            // UoW
            s.AddScoped<IUnitOfWork, EfUnitOfWork>();

            s.AddScoped<ISecurityEventSink, SecurityEventSink>();

            // 순수 싱글턴들
            s.AddSingleton<IClock, SystemClock>();
            s.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
            s.AddSingleton<ITokenService>(sp =>
            {
                var c = sp.GetRequiredService<IConfiguration>();
                var key = c["Jwt:Key"]!;
                var issuer = c["Jwt:Issuer"];
                var audience = c["Jwt:Audience"];
                return new JwtTokenService(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    issuer, audience,
                    accessTtl: TimeSpan.FromMinutes(30),
                    refreshTtl: TimeSpan.FromDays(7));
            });

            return s;
        }
    }
}
