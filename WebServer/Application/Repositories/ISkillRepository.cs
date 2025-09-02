using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface ISkillRepository
    {
        Task<Skill?> GetByIdAsync(int id, bool includeLevels, CancellationToken ct);
        Task<Skill?> GetByNameAsync(string name, CancellationToken ct);
        Task<IReadOnlyList<Skill>> ListAsync(
           SkillType? type,
           int? elementId,
           string? nameContains,
           int skip,
           int take,
           CancellationToken ct);
        Task AddAsync(Skill entity, CancellationToken ct);
        Task RemoveAsync(Skill entity, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
