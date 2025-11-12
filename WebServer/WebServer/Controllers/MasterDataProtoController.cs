using Application.Elements;
using Application.Factions;
using Application.Rarities;
using Application.Roles;
using Game.MasterData;
using Microsoft.AspNetCore.Mvc;
using WebServer.Mappers;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/masterdata")]
    public sealed class MasterDataProtoController : ControllerBase
    {
        private readonly IRarityCache _rarityCache;
        private readonly IElementCache _elementCache;
        private readonly IRoleCache _roleCache;
        private readonly IFactionCache _factionCache;

        public MasterDataProtoController(
            IRarityCache rarityCache,
            IElementCache elementCache,
            IRoleCache roleCache,
            IFactionCache factionCache)
        {
            _rarityCache = rarityCache;
            _elementCache = elementCache;
            _roleCache = roleCache;
            _factionCache = factionCache;
        }

        [HttpGet]
        public ActionResult<MasterDataBundle> GetAll()
        {
            var bundle = new MasterDataBundle
            {
                Rarities = { _rarityCache.GetAll().Select(x => x.ToProto()) },
                Elements = { _elementCache.GetAll().Select(x => x.ToProto()) },
                Roles = { _roleCache.GetAll().Select(x => x.ToProto()) },
                Factions = { _factionCache.GetAll().Select(x => x.ToProto()) }
            };

            return Ok(bundle);
        }
    }
}
