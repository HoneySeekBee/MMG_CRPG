using Application.Repositories.Contents;
using Domain.Entities.Contents;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BattlesRepository : IBattlesRepository
    {
        private readonly GameDBContext _context;

        public BattlesRepository(GameDBContext context)
        {
            _context = context;
        }

        public async Task<Battle?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _context.Battles.FindAsync(new object[] { id }, cancellationToken);

        public async Task<IReadOnlyList<Battle>> GetListAsync(CancellationToken cancellationToken = default)
            => await _context.Battles.AsNoTracking().ToListAsync(cancellationToken);

        public async Task AddAsync(Battle battle, CancellationToken cancellationToken = default)
            => await _context.Battles.AddAsync(battle, cancellationToken);

        public async Task UpdateAsync(Battle battle, CancellationToken cancellationToken = default)
            => _context.Battles.Update(battle);

        public async Task DeleteAsync(Battle battle, CancellationToken cancellationToken = default)
            => _context.Battles.Remove(battle);
    }
}
