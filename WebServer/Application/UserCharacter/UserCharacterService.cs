using Application.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserCha = Domain.Entities.User.UserCharacter;

namespace Application.UserCharacter
{
    public sealed class UserCharacterService : IUserCharacterService
    {
        private readonly ICharacterExpProvider _exp;
        private readonly IUserCharacterRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;

        public UserCharacterService(IUserCharacterRepository repo, IUnitOfWork uow, IClock clock)
      => (_repo, _uow, _clock) = (repo, uow, clock);
        public async Task<UserCharacterDto> CreateAsync(CreateUserCharacterRequest req, CancellationToken ct)
        {
            var existing = await _repo.GetAsync(req.UserId, req.CharacterId, ct);
            if (existing is not null) throw new InvalidOperationException("이미 보유한 캐릭터");

            var entity = UserCha.Create(req.UserId, (short)req.CharacterId, _clock.UtcNow);
            await _repo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return entity.ToDto();
        }
        public async Task<UserCharacterDto> GainExpAsync(GainExpRequest req, CancellationToken ct)
        {
            if (req.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(req.Amount));

            var uc = await LoadAsync(req.UserId, req.CharacterId, ct);

            var now = _clock.UtcNow;

            // -------------- 임시 규칙 시작 --------------
            const int ReqPerLevel = 100;                          // 이후 - CharacterExp로 대체
            int MaxLevel = 20 + 20 * uc.BreakThrough;             // 이후 - CharacterPromotion.MaxLevel로 대체
            // -------------- 임시 규칙 끝 --------------

            // EXP 지급
            uc.GainExp(req.Amount, now);

            // 자동 레벨업 (여러 레벨 가능)
            while (uc.TryLevelUp(ReqPerLevel, MaxLevel, now)) { /* no-op */ }

            await _uow.SaveChangesAsync(ct);
            return uc.ToDto();
        }

        public async Task<UserCharacterDto> LearnSkillAsync(LearnSkillRequest req, CancellationToken ct)
        {
            var uc = await LoadAsync(req.UserId, req.CharacterId, ct);
            uc.LearnSkill(req.SkillId, _clock.UtcNow);          // 선행조건/중복검사 도메인에서
            await _uow.SaveChangesAsync(ct);
            return uc.ToDto();
        }

        public async Task<UserCharacterDto> LevelUpSkillAsync(LevelUpSkillRequest req, CancellationToken ct)
        {
            var uc = await LoadAsync(req.UserId, req.CharacterId, ct);
            uc.LevelUpSkill(req.SkillId, req.Amount, _clock.UtcNow); // 최대레벨/재화소모 검증 도메인에서
            await _uow.SaveChangesAsync(ct);
            return uc.ToDto();
        }

        public async Task<UserCharacterDto?> GetAsync(GetUserCharacterRequest req, CancellationToken ct)
            => (await _repo.GetAsync(req.UserId, req.CharacterId, ct))?.ToDto();

        private async Task<UserCha> LoadAsync(int userId, int characterId, CancellationToken ct)
            => await _repo.GetAsync(userId, characterId, ct)
               ?? throw new InvalidOperationException("캐릭터가 없습니다.");
    }
}
