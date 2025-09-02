using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.SkillLevels
{
    public interface ISkillLevelService
    {
        Task<SkillLevelDto?> GetAsync(int skillId, int level, CancellationToken ct);
        Task<IReadOnlyList<SkillLevelDto>> ListAsync(int skillId, CancellationToken ct);

        Task<SkillLevelDto> CreateAsync(int skillId, CreateSkillLevelRequest req, CancellationToken ct);
        Task UpdateAsync(int skillId, int level, UpdateSkillLevelRequest req, CancellationToken ct);
        Task DeleteAsync(int skillId, int level, CancellationToken ct);
    }
}
