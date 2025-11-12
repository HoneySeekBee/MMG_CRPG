using Application.Repositories;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class EfUnitOfWork : IUnitOfWork
    {
        private readonly GameDBContext _db;

        public EfUnitOfWork(GameDBContext db)
        {
            _db = db;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
