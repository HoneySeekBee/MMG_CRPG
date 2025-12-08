using Application.SkillLevels;
using Domain.Entities.Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface ISkillLevelRepository
    {
        Task<SkillLevel?> GetByIdAsync(int skillId, int level, CancellationToken ct);
        Task<IReadOnlyList<SkillLevel>> ListAsync(int skillId, CancellationToken ct);

        Task AddAsync(SkillLevel entity, CancellationToken ct);
        Task RemoveAsync(SkillLevel entity, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
