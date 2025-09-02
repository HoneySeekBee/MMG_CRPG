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
        Task<PagedResult<Skill>> ListAsync(
            SkillListFilter filter,
            CancellationToken ct);
        Task AddAsync(Skill entity, CancellationToken ct);
        Task RemoveAsync(Skill entity, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
    }
    public enum SkillSortBy
    {
        Name,
        Type,
        ElementId,
        TargetingType,
        TargetSide,
        AoeShape,
        IsActive
    }

    public sealed class SkillListFilter
    {
        public SkillType? Type { get; init; }
        public int? ElementId { get; init; }
        public bool? IsActive { get; init; }
        public SkillTargetingType? TargetingType { get; init; }
        public TargetSideType? TargetSide { get; init; }
        public AoeShapeType? AoeShape { get; init; }
        public string? NameContains { get; init; }
        public string[]? TagsAll { get; init; }   // 모두 포함
        public string[]? TagsAny { get; init; }   // 하나라도 포함

        public SkillSortBy SortBy { get; init; } = SkillSortBy.Name;
        public bool Desc { get; init; } = false;

        public int Skip { get; init; } = 0;
        public int Take { get; init; } = 50; // 서버에서 최대치 캡(예: 200) 권장
    }
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int TotalCount { get; init; }
    }
}
