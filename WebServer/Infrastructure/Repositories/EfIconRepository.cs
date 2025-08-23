using Application.Repositories;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class EfIconRepository : IIconRepository
    {
        private readonly GameDBContext _db;
        public EfIconRepository(GameDBContext db) => _db = db;

        public Task<List<Icon>> GetAllAsync(CancellationToken ct)
        => _db.Icons.AsNoTracking().ToListAsync();
        public Task<Icon?> GetByIdAsync(int id, CancellationToken ct)
        => _db.Icons.FirstOrDefaultAsync(i => i.IconId == id, ct);

        public Task<Icon?> GetByKeyAsync(string key, CancellationToken ct)
        => _db.Icons.FirstOrDefaultAsync(i => i.Key == key, ct);

        public async Task AddAsync(Icon icon, CancellationToken ct)
        {
            _db.Icons.Add(icon);
            await _db.SaveChangesAsync(ct);
        }
        public async Task UpdateAsync(Icon icon, CancellationToken ct)
        {
            await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Icon icon, CancellationToken ct)
        {
            _db.Icons.Remove(icon);
            await _db.SaveChangesAsync(ct); 
        }




    }
}
