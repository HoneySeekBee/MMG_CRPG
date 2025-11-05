using Application.Repositories.Contents;
using Domain.Entities.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contents.Battles
{
    public class BattlesService : IBattlesService
    {
        private readonly IBattlesRepository _battleRepository;

        public BattlesService(IBattlesRepository battleRepository)
        {
            _battleRepository = battleRepository;
        }

        public async Task<BattleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var battle = await _battleRepository.GetByIdAsync(id, cancellationToken);
            return battle is null ? null : MapToDto(battle);
        }

        public async Task<IReadOnlyList<BattleDto>> GetListAsync(CancellationToken cancellationToken = default)
        {
            var battles = await _battleRepository.GetListAsync(cancellationToken);
            return battles.Select(MapToDto).ToList();
        }

        public async Task<int> CreateAsync(CreateBattleRequest request, CancellationToken cancellationToken = default)
        {
            var battle = new Battle(
                name: request.Name,
                sceneKey: request.SceneKey,
                active: request.Active,
                checkMulti: request.CheckMulti
            );

            await _battleRepository.AddAsync(battle, cancellationToken);

            // 여기서는 battle.Id가 저장 후에 세팅된다고 가정 (EF Core라면 SaveChanges 후 세팅됨)
            return battle.Id;
        }

        public async Task<bool> UpdateAsync(UpdateBattleRequest request, CancellationToken cancellationToken = default)
        {
            var battle = await _battleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (battle is null)
                return false;

            battle.Update(
                name: request.Name,
                sceneKey: request.SceneKey,
                active: request.Active,
                checkMulti: request.CheckMulti
            );

            await _battleRepository.UpdateAsync(battle, cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var battle = await _battleRepository.GetByIdAsync(id, cancellationToken);
            if (battle is null)
                return false;

            // 진짜 삭제
            await _battleRepository.DeleteAsync(battle, cancellationToken);
            return true;

            // 만약 소프트 삭제로 하고 싶으면:
            // battle.Deactivate();
            // await _battleRepository.UpdateAsync(battle, cancellationToken);
            // return true;
        }

        private static BattleDto MapToDto(Battle battle)
        {
            return new BattleDto
            {
                Id = battle.Id,
                Name = battle.Name,
                Active = battle.Active,
                SceneKey = battle.SceneKey,
                CheckMulti = battle.CheckMulti,
                CreatedAt = battle.CreatedAt,
                UpdatedAt = battle.UpdatedAt
            };
        }
    }
}
