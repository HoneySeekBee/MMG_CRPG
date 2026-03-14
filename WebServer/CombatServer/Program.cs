using Application.Combat;
using Application.Combat.Engine;
using Application.Repositories;
using Application.Skills;
using Application.UserCharacter;
using CombatServer.Formatters;
using CombatServer.Grpc;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<CombatDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.EnableRetryOnFailure(maxRetryCount: 3)));

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<ICombatRepository, EfCombatRepository>();
builder.Services.AddScoped<ICombatTickEngine, CombatTickEngine>();
builder.Services.AddScoped<ICombatService, CombatService>();
builder.Services.AddScoped<IMasterDataProvider, MasterDataProvider>();
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

app.UseHttpsRedirection();
app.MapControllers();
app.MapGrpcService<CombatGrpcService>();

app.Run();
