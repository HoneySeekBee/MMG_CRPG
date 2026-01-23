using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WebServer.Options;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration cfg)
    {
        var jwt = new JwtOptions();
        cfg.GetSection("Jwt").Bind(jwt);

        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                ValidateIssuer = !string.IsNullOrWhiteSpace(jwt.Issuer),
                ValidIssuer = jwt.Issuer,
                ValidateAudience = !string.IsNullOrWhiteSpace(jwt.Audience),
                ValidAudience = jwt.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.NameIdentifier
            };
        });

        services.AddAuthorization();
        services.Configure<JwtOptions>(cfg.GetSection("Jwt"));
        return services;
    }
}
