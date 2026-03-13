using Application.Combat;
using Application.Combat.Engine;
using Application.Repositories;
using Application.Skills;
using Application.UserCharacter;
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

// ── Controllers / Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
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

app.Run();
