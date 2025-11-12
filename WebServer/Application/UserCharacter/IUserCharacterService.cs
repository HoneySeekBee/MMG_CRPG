using Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UserCharacter
{
    public interface IUserCharacterService
    {
        Task<UserCharacterDto> CreateAsync(CreateUserCharacterRequest req, CancellationToken ct = default);
        Task<UserCharacterDto> GainExpAsync(GainExpRequest req, CancellationToken ct = default);
        Task<UserCharacterDto> LearnSkillAsync(LearnSkillRequest req, CancellationToken ct = default);
        Task<UserCharacterDto> LevelUpSkillAsync(LevelUpSkillRequest req, CancellationToken ct = default);
        Task<UserCharacterDto?> GetAsync(GetUserCharacterRequest req, CancellationToken ct = default);
        Task<PagedResult<UserCharacterDto>> GetListAsync(int userId, int page = 1, int pageSize = 50, CancellationToken ct = default);

    }
}
