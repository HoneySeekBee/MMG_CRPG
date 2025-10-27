using Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IUserCharacterEquipRepository
    {
        Task<UserCharacterEquip?> GetAsync(int userId, int characterId, int equipId, CancellationToken ct);
        Task<List<UserCharacterEquip>> ListByCharacterAsync(int userId, int characterId, CancellationToken ct);
        Task<UserCharacterEquip?> FindByInventoryIdAsync(long inventoryId, CancellationToken ct); // 타 캐릭 장착 확인 
        Task UpdateAsync(UserCharacterEquip entity, CancellationToken ct);
        Task AddAsync(UserCharacterEquip entity, CancellationToken ct = default);

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
