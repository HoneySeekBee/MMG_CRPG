using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Application.Contents.Stages;

namespace AdminTool.Models
{
    public sealed class StageListFilterVm
    {
        [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
        [Range(1, 200)] public int PageSize { get; set; } = 20;

        [DisplayName("챕터")] public int? Chapter { get; set; }
        [DisplayName("활성")] public bool? IsActive { get; set; }
        [DisplayName("검색어")] public string? Search { get; set; }

        // 드롭다운
        public IEnumerable<SelectListItem> Chapters { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> ActiveFlags { get; set; } = Array.Empty<SelectListItem>();
    }

    public sealed class StageSummaryVm
    {
        public int Id { get; set; }
        public int Chapter { get; set; }
        public int Order { get; set; }
        public string? Name { get; set; }
        public short RecommendedPower { get; set; }
        public short StaminaCost { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class StageIndexVm
    {
        public StageListFilterVm Filter { get; set; } = new();
        public IReadOnlyList<StageSummaryVm> Items { get; set; } = Array.Empty<StageSummaryVm>();
        public int TotalCount { get; set; }
    }

    // ─────────────────────────────────────
    // Form 탭 구성
    // ─────────────────────────────────────
    public sealed class EnemyRowVm
    {
        [DisplayName("적 캐릭터"), Range(1, int.MaxValue)] public int EnemyCharacterId { get; set; }
        [DisplayName("레벨"), Range(1, 999)] public short Level { get; set; } = 1;
        [DisplayName("슬롯"), Range(1, 9)] public short Slot { get; set; } = 1;
        [DisplayName("AI 프로파일")] public string? AiProfile { get; set; }

        // 선택 리스트
        public IEnumerable<SelectListItem> Enemies { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Slots { get; set; } =
            Enumerable.Range(1, 9).Select(i => new SelectListItem($"{i}", $"{i}")).ToList();
    }

    public sealed class WaveVm
    {
        [DisplayName("웨이브 순번"), Range(1, 99)] public short Index { get; set; } = 1;
        public List<EnemyRowVm> Enemies { get; set; } = new();
    }

    public sealed class DropRowVm
    {
        [DisplayName("아이템"), Range(1, int.MaxValue)] public int ItemId { get; set; }
        [DisplayName("확률(0~1)"), Range(0, 1)] public decimal Rate { get; set; } = 0m;
        [DisplayName("최소"), Range(0, 9999)] public short MinQty { get; set; } = 0;
        [DisplayName("최대"), Range(0, 9999)] public short MaxQty { get; set; } = 1;
        [DisplayName("첫클리어만")] public bool FirstClearOnly { get; set; }

        // 선택 리스트
        public IEnumerable<SelectListItem> Items { get; set; } = Array.Empty<SelectListItem>();
    }

    public sealed class RewardRowVm
    {
        [DisplayName("아이템"), Range(1, int.MaxValue)] public int ItemId { get; set; }
        [DisplayName("수량"), Range(1, 9999)] public short Qty { get; set; } = 1;

        public IEnumerable<SelectListItem> Items { get; set; } = Array.Empty<SelectListItem>();
    }

    public sealed class RequirementRowVm
    {
        [DisplayName("선행 스테이지")] public int? RequiredStageId { get; set; }
        [DisplayName("최소 계정레벨"), Range(1, 999)] public short? MinAccountLevel { get; set; }

        // 선택 리스트
        public IEnumerable<SelectListItem> Stages { get; set; } = Array.Empty<SelectListItem>();
    }
    public sealed class BatchVm
    {
        [DisplayName("선행 스테이지")] public int? RequiredStageId { get; set; }
        [DisplayName("최소 계정레벨"), Range(1, 999)] public short? MinAccountLevel { get; set; }

        // 선택 리스트
        public IEnumerable<SelectListItem> Stages { get; set; } = Array.Empty<SelectListItem>();
    }


    public sealed class StageFormVm
    {
        public int? Id { get; set; }

        // Basic
        [DisplayName("챕터"), Range(1, 999)] public int Chapter { get; set; } = 1;
        [DisplayName("순서"), Range(1, 999)] public int Order { get; set; } = 1;
        [DisplayName("이름"), MaxLength(64)] public string? Name { get; set; }
        [DisplayName("권장 전투력"), Range(0, 99999)] public short RecommendedPower { get; set; }
        [DisplayName("소모 스태미나"), Range(0, 999)] public short StaminaCost { get; set; } = 6;
        [DisplayName("활성")] public bool IsActive { get; set; } = true;

        // Tabs
        public List<WaveVm> Waves { get; set; } = new();
        public List<DropRowVm> Drops { get; set; } = new();
        public List<RewardRowVm> FirstRewards { get; set; } = new();
        public List<RequirementRowVm> Requirements { get; set; } = new();
        public List<BatchVm> Batches { get; set; } = new();

        // 드롭다운 데이터
        public IEnumerable<SelectListItem> ChapterOptions { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> EnemyOptions { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> ItemOptions { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> StageOptions { get; set; } = Array.Empty<SelectListItem>();
    }

    // ─────────────────────────────────────
    // Mapper (DTO/Request ↔ VM)
    // ─────────────────────────────────────
    public static class StageVmMapper
    {
        // List DTO → VM
        public static StageSummaryVm FromDto(StageSummaryDto d) =>
            new()
            {
                Id = d.Id,
                Chapter = d.Chapter,
                Order = d.StageNum,
                Name = d.Name,
                RecommendedPower = d.RecommendedPower,
                StaminaCost = d.StaminaCost,
                IsActive = d.IsActive
            };

        // Detail DTO → Form VM
        public static StageFormVm FromDetailDto(StageDetailDto d,
            IEnumerable<SelectListItem>? enemyOptions = null,
            IEnumerable<SelectListItem>? itemOptions = null,
            IEnumerable<SelectListItem>? stageOptions = null,
            IEnumerable<SelectListItem>? chapterOptions = null)
        {
            var vm = new StageFormVm
            {
                Id = d.Id,
                Chapter = d.Chapter,
                Order = d.Order,
                Name = d.Name,
                RecommendedPower = d.RecommendedPower,
                StaminaCost = d.StaminaCost,
                IsActive = d.IsActive,
                ChapterOptions = chapterOptions ?? Array.Empty<SelectListItem>(),
                EnemyOptions = enemyOptions ?? Array.Empty<SelectListItem>(),
                ItemOptions = itemOptions ?? Array.Empty<SelectListItem>(),
                StageOptions = stageOptions ?? Array.Empty<SelectListItem>()
            };

            vm.Waves = d.Waves
                .OrderBy(w => w.Index)
                .Select(w => new WaveVm
                {
                    Index = w.Index,
                    Enemies = w.Enemies.Select(e => new EnemyRowVm
                    {
                        EnemyCharacterId = e.EnemyCharacterId,
                        Level = e.Level,
                        Slot = e.Slot,
                        AiProfile = e.AiProfile,
                        Enemies = vm.EnemyOptions // 전달한 셀렉트 재사용
                    }).ToList()
                }).ToList();

            vm.Drops = d.Drops.Select(x => new DropRowVm
            {
                ItemId = x.ItemId,
                Rate = x.Rate,
                MinQty = x.MinQty,
                MaxQty = x.MaxQty,
                FirstClearOnly = x.FirstClearOnly,
                Items = vm.ItemOptions
            }).ToList();

            vm.FirstRewards = d.FirstRewards.Select(x => new RewardRowVm
            {
                ItemId = x.ItemId,
                Qty = x.Qty,
                Items = vm.ItemOptions
            }).ToList();

            vm.Requirements = d.Requirements.Select(x => new RequirementRowVm
            {
                RequiredStageId = x.RequiredStageId,
                MinAccountLevel = x.MinAccountLevel,
                Stages = vm.StageOptions
            }).ToList();

            return vm;
        }

        // Create
        public static CreateStageRequest ToCreateRequest(this StageFormVm vm) =>
            new(
                Chapter: vm.Chapter,
                StageNumer: vm.Order,
                RecommendedPower: vm.RecommendedPower,
                StaminaCost: vm.StaminaCost,
                IsActive: vm.IsActive,
                Waves: vm.Waves
                    .OrderBy(w => w.Index)
                    .Select(w => new WaveCmd(
                        w.Index,
                        w.Enemies.Select(e => new EnemyCmd(e.EnemyCharacterId, e.Level, e.Slot, e.AiProfile)).ToList()
                    )).ToList(),
                Drops: vm.Drops.Select(d => new DropCmd(d.ItemId, d.Rate, d.MinQty, d.MaxQty, d.FirstClearOnly)).ToList(),
                FirstRewards: vm.FirstRewards.Select(r => new RewardCmd(r.ItemId, r.Qty)).ToList(),
                Requirements: vm.Requirements.Select(r => new RequirementCmd(r.RequiredStageId, r.MinAccountLevel)).ToList(),
                Batches: vm.Batches.Select(r => new BatchCmd()).ToList() 
            );

        // Update
        public static UpdateStageRequest ToUpdateRequest(this StageFormVm vm, int id) =>
            new(
                Id: id,
                Chapter: vm.Chapter,
                StageNumer: vm.Order,
                RecommendedPower: vm.RecommendedPower,
                StaminaCost: vm.StaminaCost,
                IsActive: vm.IsActive,
                

                Waves: vm.Waves
                    .OrderBy(w => w.Index)
                    .Select(w => new WaveCmd(
                        w.Index,
                        w.Enemies.Select(e => new EnemyCmd(e.EnemyCharacterId, e.Level, e.Slot, e.AiProfile)).ToList()
                    )).ToList(),
                Drops: vm.Drops.Select(d => new DropCmd(d.ItemId, d.Rate, d.MinQty, d.MaxQty, d.FirstClearOnly)).ToList(),
                FirstRewards: vm.FirstRewards.Select(r => new RewardCmd(r.ItemId, r.Qty)).ToList(),
                Requirements: vm.Requirements.Select(r => new RequirementCmd(r.RequiredStageId, r.MinAccountLevel)).ToList(),

                Batches: vm.Batches.Select(r => new BatchCmd()).ToList()
            );
    }
}
