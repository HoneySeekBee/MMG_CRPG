using Application.Combat;
using Application.Combat.Engine;
using Application.Repositories;
using Application.Skills;
using Application.UserCharacter;
using CombatServer.Formatters;
using CombatServer.Grpc;
using CombatServer.HostedServices;
using Infrastructure.Caching;
using Infrastructure.Persistence;
using Infrastructure.Reader;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Port 80: HTTP/1.1 for REST (Unity client via nginx)
// Port 5001: HTTP/2 for gRPC (WebServer internal call)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
    options.ListenAnyIP(5001, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

// ── Redis ─────────────────────────────────────────────────────────────────────
var redisConn = builder.Configuration.GetValue<string>("Redis")
    ?? throw new InvalidOperationException("Redis connection string is not configured.");
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));
builder.Services.AddHostedService<CombatServerRegistryService>();

// ── Database ──────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("GameDb")
    ?? throw new InvalidOperationException("Connection string 'GameDb' is not configured.");

builder.Services.AddDbContext<CombatDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.EnableRetryOnFailure(maxRetryCount: 3)));
builder.Services.AddDbContextFactory<CombatDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<ICombatRepository, EfCombatRepository>();
builder.Services.AddScoped<ICombatTickEngine, CombatTickEngine>();
builder.Services.AddScoped<ICombatService, CombatService>();
builder.Services.AddScoped<IMasterDataProvider, MasterDataProvider>();
builder.Services.AddScoped<IStageReader, EfStageReader>();
builder.Services.AddScoped<ICharacterReader, EfCharacterReader>();
builder.Services.AddScoped<ISkillReader, EfSkillReader>();
builder.Services.AddScoped<IUserCharacterReader, EfUserCharacterReader>();
builder.Services.AddScoped<IMonsterStatReader, EfMonsterStatReader>();
builder.Services.AddSingleton<ISkillCache, EfSkillCache>();
builder.Services.AddGrpc(); 

// ── Controllers / Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers(opts =>
{
    opts.InputFormatters.Insert(0, new ProtobufInputFormatter());
    opts.OutputFormatters.Insert(0, new ProtobufOutputFormatter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGrpcService<CombatGrpcService>();

// Warm up skill cache before accepting traffic
await app.Services.GetRequiredService<ISkillCache>().ReloadAsync();

app.Run();
