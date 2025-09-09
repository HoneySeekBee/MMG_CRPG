using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface ISecurityEventRepository
    {
        Task AddAsync(SecurityEvent e, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
    public interface IPasswordHasher
    {
        string Hash(string plain);
        bool Verify(string plain, string hash);
    }

    public interface ITokenService
    {
        (string token, DateTimeOffset expiresAt) CreateAccessToken(User user);
        (string token, DateTimeOffset expiresAt) CreateRefreshToken(User user);
        string Hash(string token);
    }

    public interface IClock { DateTimeOffset UtcNow { get; } }
}
