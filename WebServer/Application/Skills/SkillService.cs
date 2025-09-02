using Application.Repositories;
using Application.Validation;
using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Skills
{
    public sealed class SkillService : ISkillService
    {
        private readonly ISkillRepository _repo;

        public SkillService(ISkillRepository repo) => _repo = repo;

        // 단건 (레벨 미포함)
        public async Task<SkillDto?> GetAsync(int id, CancellationToken ct)
            => (await _repo.GetByIdAsync(id, includeLevels: false, ct)) is { } e
                ? SkillDto.From(e)
                : null;

        // 단건 (레벨 포함)
        public async Task<SkillWithLevelsDto?> GetWithLevelsAsync(int id, CancellationToken ct)
            => (await _repo.GetByIdAsync(id, includeLevels: true, ct)) is { } e
                ? SkillWithLevelsDto.From(e)
                : null;

        // 목록 (요약 DTO 반환)
        public async Task<IReadOnlyList<SkillListItemDto>> ListAsync(
            SkillType? type,
            int? elementId,
            string? nameContains,
            int page,
            int pageSize,
            CancellationToken ct)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 50;

            var skip = (page - 1) * pageSize;

            // Repo는 필터/페이징을 적용해 도메인 엔티티 리스트 반환한다고 가정
            var list = await _repo.ListAsync(
                type: type,
                elementId: elementId,
                nameContains: nameContains?.Trim(),
                skip: skip,
                take: pageSize,
                ct: ct);

            return list.Select(SkillListItemDto.From).ToList();
        }

        // 생성
        public async Task<SkillDto> CreateAsync(CreateSkillRequest req, CancellationToken ct)
        {
            Guard.NotEmpty(req.Name, nameof(req.Name));

            // (선택) 이름 중복 체크가 필요하면 Repo에 메서드 추가해서 사용
            // if (await _repo.GetByNameAsync(req.Name.Trim(), ct) is not null)
            //     throw new InvalidOperationException("이미 존재하는 Skill 이름입니다.");
            var e = new Skill(
    0,
    req.Name.Trim(),
    req.Type,
    req.ElementId,
    req.IconId
);

            await _repo.AddAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
            return SkillDto.From(e);
        }

        // 기본정보 수정 (이름/타입/속성/아이콘)
        public async Task UpdateAsync(int id, UpdateSkillBasicsRequest req, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, includeLevels: false, ct)
                    ?? throw new KeyNotFoundException("대상을 찾을 수 없습니다.");

            Guard.NotEmpty(req.Name, nameof(req.Name));

            e.Name = req.Name.Trim();
            e.Type = req.Type;
            e.ElementId = req.ElementId;
            e.IconId = req.IconId;

            await _repo.SaveChangesAsync(ct);
        }

        // 이름만 경량 수정
        public async Task RenameAsync(int id, RenameSkillRequest req, CancellationToken ct)
        {
            Guard.NotEmpty(req.Name, nameof(req.Name));

            var e = await _repo.GetByIdAsync(id, includeLevels: false, ct)
                    ?? throw new KeyNotFoundException("대상을 찾을 수 없습니다.");

            e.Name = req.Name.Trim();
            await _repo.SaveChangesAsync(ct);
        }

        // 삭제
        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, includeLevels: false, ct)
                    ?? throw new KeyNotFoundException("대상을 찾을 수 없습니다.");

            await _repo.RemoveAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
        }
    }
}
