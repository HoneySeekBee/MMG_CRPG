using Application.UserCharacter;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using WebServer.Mappers;

namespace Presentation.Controllers.Game.User
{
    [ApiController]
    [Route("api/pb/userCharacters")]
    [Produces("application/x-protobuf")]
    public class GameUserCharacterController : ControllerBase
    {
        private readonly IUserCharacterService _svc;

        public GameUserCharacterController(IUserCharacterService svc)
        {
            _svc = svc;
        }

        // GET /api/pb/userCharacters/{userId} 
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetAll(int userId, CancellationToken ct = default)
        {
            var paged = await _svc.GetListAsync(userId, 1, int.MaxValue, ct);

            var pb = paged.Items.ToPb();

            return new FileContentResult(pb.ToByteArray(), "application/x-protobuf");
        }



    }
}
