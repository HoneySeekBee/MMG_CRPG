using Application.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ElementAffinities
{
    public class ElementAffinityService : IElementAffinityService
    {
        private readonly IElementAffinityRepository _repo;
        private readonly IElementRepository _elemRepo;

        public ElementAffinityService(IElementAffinityRepository repo, IElementRepository elemRepo)
        {
            _repo = repo; _elemRepo = elemRepo;
        }

        public async Task<ElementAffinityDto?> GetAsync(int attacker, int defender, CancellationToken ct)
        {
            var e = await _repo.GetAsync(attacker, defender, ct);
            return e is null ? null : ElementAffinityDto.From(e);
        }

        public async Task<IReadOnlyList<ElementAffinityDto>> ListAsync(
            int? attacker, int? defender, int page, int pageSize, CancellationToken ct)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 50;

            var list = await _repo.ListAsync(attacker, defender, (page - 1) * pageSize, pageSize, ct);
            return list.Select(ElementAffinityDto.From).ToList();
        }
        public async Task CreateAsync(CreateElementAffinityRequest req, CancellationToken ct)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (req.AttackerElementId == req.DefenderElementId)
                throw new InvalidOperationException("자기 자신 상성은 만들 수 없습니다.");

            var allowed = new[] { 0.50m, 0.75m, 1.00m, 1.25m, 1.50m };
            if (!allowed.Contains(req.Multiplier))
                throw new InvalidOperationException("Multiplier 값은 0.50, 0.75, 1.00, 1.25, 1.50 중 하나여야 합니다.");

            // 존재 확인
            if (await _elemRepo.GetByIdAsync(req.AttackerElementId, ct) is null)
                throw new KeyNotFoundException("공격 속성이 존재하지 않습니다.");
            if (await _elemRepo.GetByIdAsync(req.DefenderElementId, ct) is null)
                throw new KeyNotFoundException("방어 속성이 존재하지 않습니다.");

            // 중복 방지
            if (await _repo.GetAsync(req.AttackerElementId, req.DefenderElementId, ct) is not null)
                throw new InvalidOperationException("해당 (공격,방어) 상성이 이미 존재합니다.");

            // 저장
            var entity = new ElementAffinity
            {
                AttackerElementId = req.AttackerElementId,
                DefenderElementId = req.DefenderElementId,
                Multiplier = req.Multiplier
            };

            await _repo.AddAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(int attacker, int defender, UpdateElementAffinityRequest req, CancellationToken ct)
        {
            var entity = await _repo.GetAsync(attacker, defender, ct)
                ?? throw new KeyNotFoundException("상성을 찾을 수 없습니다.");

            if (req.Multiplier < 0m || req.Multiplier > 10m)
                throw new InvalidOperationException("Multiplier 범위가 잘못되었습니다 (0~10).");

            entity.Multiplier = req.Multiplier;
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int attacker, int defender, CancellationToken ct)
        {
            var entity = await _repo.GetAsync(attacker, defender, ct)
                ?? throw new KeyNotFoundException("상성을 찾을 수 없습니다.");

            await _repo.RemoveAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);
        }
    }
}
