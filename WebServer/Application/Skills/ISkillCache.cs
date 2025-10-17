using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Skills
{
    public interface ISkillCache
    {
        IReadOnlyList<SkillWithLevelsDto> GetAll();
        SkillWithLevelsDto? GetById(int id); 
        Task ReloadAsync(CancellationToken ct = default);
    }
}
