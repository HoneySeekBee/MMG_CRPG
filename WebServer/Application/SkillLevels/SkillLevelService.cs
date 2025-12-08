using Application.Repositories;
using Domain.Entities;
using Domain.Entities.Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.SkillLevels
{
    public sealed class SkillLevelService : ISkillLevelService
    {
        private readonly ISkillLevelRepository _repo;

        public SkillLevelService(ISkillLevelRepository repo) => _repo = repo;

        public async Task<SkillLevelDto?> GetAsync(int skillId, int level, CancellationToken ct)
        {
            Console.WriteLine($"[ WebAPI ] - GetSkill | SkillId : {skillId}");
            var e = await _repo.GetByIdAsync(skillId, level, ct);
            return e is null ? null : SkillLevelDto.From(e);
        }

        public async Task<IReadOnlyList<SkillLevelDto>> ListAsync(int skillId, CancellationToken ct)
        {
            Console.WriteLine($"[ WebAPI ] - GetList | SkillId : {skillId}");
            var list = await _repo.ListAsync(skillId, ct);
            return list.Select(SkillLevelDto.From).ToList();
        }

        public async Task<SkillLevelDto> CreateAsync(int skillId, CreateSkillLevelRequest req, CancellationToken ct)
        {
            // 기본 검증
            if (req.Level <= 0) throw new ArgumentOutOfRangeException(nameof(req.Level));
            if (req.CostGold < 0) throw new ArgumentOutOfRangeException(nameof(req.CostGold));

            // 중복 방지
            if (await _repo.GetByIdAsync(skillId, req.Level, ct) is not null)
                throw new InvalidOperationException("이미 존재하는 레벨입니다.");

            var e = new SkillLevel(
                skillId: skillId,
                level: req.Level,
                values: req.Values,
                description: req.Description,
                materials: req.Materials,
                costGold: req.CostGold
            );

            await _repo.AddAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
            return SkillLevelDto.From(e);
        }

        public async Task UpdateAsync(int skillId, int level, UpdateSkillLevelRequest req, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(skillId, level, ct)
                ?? throw new KeyNotFoundException("대상을 찾을 수 없습니다.");

            if (req.CostGold < 0) throw new ArgumentOutOfRangeException(nameof(req.CostGold));

            e.Update(
                values: req.Values,
                description: req.Description,
                materials: req.Materials,
                costGold: req.CostGold
            );

            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int skillId, int level, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(skillId, level, ct)
                ?? throw new KeyNotFoundException("대상을 찾을 수 없습니다.");

            await _repo.RemoveAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
        }
    }
}
