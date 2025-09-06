using AdminTool.Models;
using Application.Combat;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using AdminTool.Services;
using System;

namespace AdminTool.Controllers
{
    [Route("Admin/Combat")]
    public sealed class CombatController : Controller
    {
        private readonly ICombatApiClient _combat;
        private readonly IStageUiProvider _stages; // 스테이지 드롭다운 제공(간단 인터페이스)
        private readonly ICharacterUiProvider _chars;

        public CombatController(ICombatApiClient combat, IStageUiProvider stages, ICharacterUiProvider chars)
        {
            _combat = combat; _stages = stages; _chars = chars;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var vm = new CombatVm
            {
                StageOptions = await _stages.GetOptionsAsync(ct),
                CharacterOptions = await _chars.GetOptionsAsync(ct) // 캐릭터 옵션
            };
            vm.Party.Add(new CombatPartyRowVm { CharacterId = 1, Level = 1 });
            return View(vm);
        }


        [HttpPost("Simulate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Simulate(CombatVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                vm.StageOptions = await _stages.GetOptionsAsync(ct);
                vm.CharacterOptions = await _chars.GetOptionsAsync(ct);
                return View("Index", vm);
            }

            try
            {
                var req = vm.ToSimulateRequest();
                var res = await _combat.SimulateAsync(req, ct);
                vm.Result = CombatVmMapper.FromResponse(res);

                var page = await _combat.GetLogAsync(vm.Result.CombatId, null, 200, ct);
                vm.LogPage = CombatVmMapper.FromPageDto(page);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            vm.StageOptions = await _stages.GetOptionsAsync(ct);
            vm.CharacterOptions = await _chars.GetOptionsAsync(ct);
            return View("Index", vm);
        }

        [HttpGet("Log")]
        public async Task<IActionResult> Log(long combatId, string? cursor, int size = 200, CancellationToken ct = default)
        {
            var page = await _combat.GetLogAsync(combatId, cursor, size, ct);
            var vm = CombatVmMapper.FromPageDto(page);
            return PartialView("_CombatLogRows", vm);
        }
    }

    //
    public interface IStageUiProvider
    {
        Task<IEnumerable<SelectListItem>> GetOptionsAsync(CancellationToken ct);
    }
}
