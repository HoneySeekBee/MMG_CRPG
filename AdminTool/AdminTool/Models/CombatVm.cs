using Application.Combat;
using Domain.Entities;
using Domain.Events;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AdminTool.Models
{
    public class CombatVm
    {
        // 입력 관련 

        [DisplayName("스테이지"), Range(1, int.MaxValue)]
        public int StageId { get; set; }
        [DisplayName("시드(선택)")]
        public long? Seed { get; set; }

        [DisplayName("클라 버전")]
        public string? ClientVersion { get; set; }

        [DisplayName("파티")]
        [MinLength(1, ErrorMessage = "파티에 최소 1명 이상 필요합니다.")]
        public List<CombatPartyRowVm> Party { get; set; } = new();

        [DisplayName("스킬 입력(선택)")]
        public List<CombatSkillInputRowVm> SkillInputs { get; set; } = new();


        public CombatSimulateResultVm? Result { get; set; }
        public CombatLogPageVm? LogPage { get; set; }
        public IEnumerable<SelectListItem> StageOptions { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> CharacterOptions { get; set; } = Enumerable.Empty<SelectListItem>();
        public bool HasResult => Result != null;
        public bool HasLog => LogPage != null && LogPage.Items.Count > 0;
    }
    public sealed class CombatPartyRowVm
    {
        [DisplayName("캐릭터 ID"), Range(1, long.MaxValue)]
        public long CharacterId { get; set; }

        [DisplayName("레벨"), Range(1, 999)]
        public int Level { get; set; }
    }
    public sealed class CombatSkillInputRowVm
    {
        [DisplayName("시간(ms)"), Range(0, int.MaxValue)]
        public int TMs { get; set; }

        [DisplayName("시전자 참조"), Required] // 예: ally:1
        public string CasterRef { get; set; } = string.Empty;

        [DisplayName("스킬 ID"), Range(1, long.MaxValue)]
        public long SkillId { get; set; }

        [DisplayName("타깃(CSV)")]
        public string? TargetsCsv { get; set; }
    }
    public sealed class CombatSimulateResultVm
    {
        public long CombatId { get; set; }
        public string Result { get; set; } = ""; // "win" | "lose" | "error"
        public int? ClearMs { get; set; }
        public string? BalanceVersion { get; set; }
        public string? ClientVersion { get; set; }

        // 응답 내 이벤트 프리뷰(최대 200개)
        public List<CombatLogEventVm> Events { get; set; } = new();
    }
    public sealed class CombatLogEventVm
    {
        public int TMs { get; set; }
        public string Type { get; set; } = "";
        public string Actor { get; set; } = "";
        public string? Target { get; set; }
        public int? Damage { get; set; }
        public bool? Crit { get; set; }
        public string? ExtraJson { get; set; } // 펼침용 JSON 문자열
    }
    public sealed class CombatLogPageVm
    {
        public long CombatId { get; set; }
        public List<CombatLogEventVm> Items { get; set; } = new();
        public string? NextCursor { get; set; }
        public bool HasMore => !string.IsNullOrEmpty(NextCursor);
    }

    public static class CombatVmMapper
    {
        // --- 폼 -> Application 요청 ---
        public static SimulateCombatRequest ToSimulateRequest(this CombatVm vm)
        {
            var party = vm.Party.ConvertAll(p => new PartyMemberDto(p.CharacterId, p.Level));
            var skills = vm.SkillInputs?.Select(s =>
            {
                var targets = string.IsNullOrWhiteSpace(s.TargetsCsv)
                    ? Array.Empty<string>()
                    : s.TargetsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return new SkillInputDto(s.TMs, s.CasterRef, s.SkillId, targets);
            }).ToList() ?? new List<SkillInputDto>();

            return new SimulateCombatRequest(
                StageId: vm.StageId,
                Seed: vm.Seed,
                Party: party,
                SkillInputs: skills,
                ClientVersion: vm.ClientVersion
            );
        }

        // --- Application 응답 -> Result VM ---
        public static CombatSimulateResultVm FromResponse(SimulateCombatResponse res)
            => new()
            {
                CombatId = res.CombatId,
                Result = res.Result,
                ClearMs = res.ClearMs,
                BalanceVersion = res.BalanceVersion,
                ClientVersion = res.ClientVersion,
                Events = res.Events.Select(ToEventVm).ToList()
            };

        // --- Application 로그 페이지 -> VM ---
        public static CombatLogPageVm FromPageDto(CombatLogPageDto page)
            => new()
            {
                CombatId = page.CombatId,
                Items = page.Items.Select(ToEventVm).ToList(),
                NextCursor = page.NextCursor
            };

        private static CombatLogEventVm ToEventVm(CombatLogEventDto e)
            => new()
            {
                TMs = e.TMs,
                Type = e.Type,
                Actor = e.Actor,
                Target = e.Target,
                Damage = e.Damage,
                Crit = e.Crit,
                ExtraJson = e.Extra is null ? null : JsonSerializer.Serialize(e.Extra)
            };
    }
}
