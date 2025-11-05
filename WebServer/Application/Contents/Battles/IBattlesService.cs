using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Battles
{
    public interface IBattlesService
    {
        Task<BattleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<BattleDto>> GetListAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(CreateBattleRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(UpdateBattleRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default); // 삭제 말고 Active를 false로 한다. 
    }
}
