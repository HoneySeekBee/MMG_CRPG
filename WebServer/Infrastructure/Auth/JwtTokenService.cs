using Application.Repositories;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Auth
{
    public sealed class JwtTokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly string? _issuer;
        private readonly string? _audience;
        private readonly TimeSpan _accessTtl;
        private readonly TimeSpan _refreshTtl;

        public JwtTokenService(
            SymmetricSecurityKey key,
            string? issuer,
            string? audience,
            TimeSpan accessTtl,
            TimeSpan refreshTtl)
        {
            _key = key;
            _issuer = issuer;
            _audience = audience;
            _accessTtl = accessTtl;
            _refreshTtl = refreshTtl;
        }

        public (string, DateTimeOffset) CreateAccessToken(User user)
        {
            var now = DateTimeOffset.UtcNow;
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier,   user.Id.ToString()),
                new Claim("account",                   user.Account)
            };

            var jwt = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: now.UtcDateTime,
                expires: (now + _accessTtl).UtcDateTime,
                signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return (token, now + _accessTtl);
        }
        public (string, DateTimeOffset) CreateRefreshToken(User user)
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(bytes);
            var exp = DateTimeOffset.UtcNow + _refreshTtl;
            return (token, exp);
        }


        public string Hash(string token)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}
