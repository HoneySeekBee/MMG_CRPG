using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class SecurityEventRepository : ISecurityEventRepository
    {
        private readonly GameDBContext _db;
        public SecurityEventRepository(GameDBContext db) => _db = db;

        public Task AddAsync(SecurityEvent e, CancellationToken ct)
        {
            _db.SecurityEvents.Add(e);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
