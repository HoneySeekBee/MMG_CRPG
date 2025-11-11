using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Monsters;

namespace Application.Monsters
{
    public interface IMonsterRepository
    {
        Task<List<Monster>> GetAllAsync(CancellationToken ct = default);
        Task<Monster?> GetByIdAsync(int id, CancellationToken ct = default);
        Task AddAsync(Monster monster, CancellationToken ct = default);
        Task DeleteAsync(Monster monster, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
