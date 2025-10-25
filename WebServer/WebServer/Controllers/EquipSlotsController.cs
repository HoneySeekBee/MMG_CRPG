using Application.EquipSlots;
using Microsoft.AspNetCore.Mvc;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/equipslots")]
    public sealed class EquipSlotsController : ControllerBase
    {
        private readonly IEquipSlotsService _svc;
        public EquipSlotsController(IEquipSlotsService svc) => _svc = svc;

        // 운영툴 조회용: 전체 목록
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<EquipSlotDto>>> GetAll(CancellationToken ct)
        {
            var items = await _svc.GetAllAsync(ct);
            return Ok(items);
        }
    }
}
