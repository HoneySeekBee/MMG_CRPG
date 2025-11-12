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

        // 목록
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

            var list = await _repo.ListAsync(
                type: type,
                elementId: elementId,
                nameContains: nameContains?.Trim(),
                skip: skip,
                take: pageSize,
                ct: ct);

            return list.Select(SkillListItemDto.From).ToList();
        }

        // 생성// 생성
        public async Task<SkillDto> CreateAsync(CreateSkillRequest req, CancellationToken ct)
        {
            Guard.NotEmpty(req.Name, nameof(req.Name));

            var e = new Skill(
                0,
                req.Name.Trim(),
                req.Type,
                req.ElementId,
                req.IconId,
                req.TargetingType,
                req.AoeShape,
                req.TargetSide,
                isActive: req.IsActive?? true,           // ← 누락1
                baseInfo: req.BaseInfo,           // ← 누락2
                tags: req.Tag                     // ← 누락3
            );


            await _repo.AddAsync(e, ct);
            await _repo.SaveChangesAsync(ct);
            return SkillDto.From(e);
        }
        // 기본정보 수정 (이름/타입/속성/아이콘)
        public async Task UpdateAsync(int id, UpdateSkillBasicsRequest req, CancellationToken ct)
        {
            Console.WriteLine($"[API] (SkillService) : basic - name : {req.Name}, iconId : {req.IconId}");
            var e = await _repo.GetByIdAsync(id, includeLevels: false, ct)
                    ?? throw new KeyNotFoundException("대상을 찾을 수 없습니다.");

            Guard.NotEmpty(req.Name, nameof(req.Name));

            e.Rename(req.Name.Trim());
            e.IconId = req.IconId;

            await _repo.SaveChangesAsync(ct);
        }
        // 전투 속성 수정
        public async Task UpdateCombatAsync(int id, UpdateSkillCombatRequest req, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, includeLevels: false, ct)
                    ?? throw new KeyNotFoundException("대상을 찾을 수 없습니다.");

            // 값 적용
            e.Type = req.Type;
            e.ElementId = req.ElementId;

            // 액티브/패시브 및 타게팅/범위 일관성 확보
            e.IsActive = req.IsActive;
            e.TargetSide = req.TargetSide;

            if (!e.IsActive)
            {
                // 패시브면 타게팅/AOE 없음
                e.TargetingType = SkillTargetingType.None;
                e.AoeShape = AoeShapeType.None;
            }
            else
            {
                e.TargetingType = req.TargetingType;
                e.AoeShape = req.AoeShape;
            }

            await _repo.SaveChangesAsync(ct);
        }
        public async Task UpdateMetaAsync(int id, PatchSkillMetaRequest req, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, includeLevels: false, ct)
                    ?? throw new KeyNotFoundException("대상을 찾을 수 없습니다.");

            // Tag 정규화
            if (req.NormalizeTags && req.Tag is { Length: > 0 })
                e.SetTags(req.Tag
                    .Select(t => (t ?? "").Trim().ToLowerInvariant())
                    .Where(t => t.Length > 0)
                    .Distinct()
                    .ToArray());
            else if (req.Tag is not null)
                e.SetTags(req.Tag);


            // BaseInfo 교체 (merge가 필요하면 여기서 기존 e.BaseInfo와 병합 로직 추가)
            e.BaseInfo = req.BaseInfo;


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
