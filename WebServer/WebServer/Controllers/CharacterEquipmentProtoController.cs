using Microsoft.AspNetCore.Mvc;
using Contracts.Protos;
using Application.UserCharacterEquips;
using WebServer.Mappers;
using Google.Protobuf;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/users/{userId:int}/characters/{characterId:long}/equipment")]
    [Produces("application/x-protobuf")]
    public class CharacterEquipmentProtoController : ControllerBase
    {
        private readonly ICharacterEquipmentService _svc;

        public CharacterEquipmentProtoController(ICharacterEquipmentService svc)
        {
            _svc = svc;
        }

        // 현재 캐릭터 장비 상태 조회 
        [HttpGet]
        public async Task<IActionResult> Get(int userId, int characterId, CancellationToken ct = default)
        {
            var snapshot = await _svc.GetAsync(userId, characterId, ct);
            var pb = snapshot.ToPbGet(); // -> GetCharacterEquipmentResponse
            return File(pb.ToByteArray(), "application/x-protobuf");
        }

        // 아이템 장착 
        // GET /api/pb/users/{userId}/characters/{characterId}/equipment
        [HttpPut("{equipId:int}")]
        [Consumes("application/x-protobuf")]
        public async Task<IActionResult> Set(int userId, int characterId, int equipId, [FromBody] SetEquipmentRequest body, CancellationToken ct = default)
        { 
            if (body.EquipId != 0 && body.EquipId != equipId)
                return BadRequest();

            var cmd = new SetEquipmentCommand(
                UserId: userId,
                CharacterId: characterId,
                EquipId: equipId,
                InventoryId: body.HasInventoryId ? body.InventoryId : (long?)null // 해제면 null
            );

            // 최소 구현: 예외 매핑 없이 바로 처리
            var result = await _svc.SetAsync(cmd, ct);
            var pb = result.ToPbSet(); // -> SetEquipmentResponse
            return File(pb.ToByteArray(), "application/x-protobuf");

        } 
    }
}
