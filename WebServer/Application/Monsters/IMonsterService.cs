using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Monsters
{
    public interface IMonsterService
    {
        Task<List<MonsterDto>> GetAllAsync(CancellationToken ct = default);
        Task<MonsterDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<int> CreateAsync(CreateMonsterRequest request, CancellationToken ct = default);
        Task UpdateAsync(UpdateMonsterRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task UpsertStatAsync(UpsertMonsterStatRequest request, CancellationToken ct = default);
    }
}
