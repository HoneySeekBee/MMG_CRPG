using Application.CharacterModels;
using Application.Repositories;
using Contracts.CharacterModel;
using Microsoft.AspNetCore.Mvc;
using WebServer.Mappers;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/character-model")]
    [Produces("application/x-protobuf")]
    [Consumes("application/x-protobuf")]
    public sealed class CharacterModelProtoController : ControllerBase
    {
        private readonly ICharacterModelRepository _repo;
        private readonly ICharacterModelCache _cache;

        public CharacterModelProtoController(ICharacterModelRepository repo, ICharacterModelCache cache)
        {
            _repo = repo;
            _cache = cache;
        }
        [HttpGet("{characterId:int}")]
        public async Task<GetCharacterModelResponsePb> GetById([FromRoute] int characterId, CancellationToken ct)
        {
            var entity = await _repo.GetByCharacterIdAsync(characterId, ct)
                         ?? throw new Exception("Character model not found");

            return new GetCharacterModelResponsePb
            {
                Model = entity.ToProto()
            };
        }
        [HttpPost("list")]
        public async Task<ListCharacterModelsResponsePb> List([FromBody] ListCharacterModelsRequestPb req, CancellationToken ct)
        {
            await Task.CompletedTask;

            IEnumerable<CharacterModelDto> src;
            if (req.CharacterIds == null || req.CharacterIds.Count == 0)
                src = _cache.GetAllModels();           // 전체
            else
                src = req.CharacterIds
                        .Select(id => _cache.GetModel(id))
                        .Where(m => m != null)!
                        .Cast<CharacterModelDto>();

            var res = new ListCharacterModelsResponsePb();
            res.Models.AddRange(src.Select(m => m.ToProto()));
            return res;
        }

        [HttpGet("parts")]
        public async Task<ListCharacterModelPartsResponsePb> ListParts(CancellationToken ct)
        {
            await Task.CompletedTask;
            var res = new ListCharacterModelPartsResponsePb();
            res.Parts.AddRange(_cache.GetAllParts().Select(p => p.ToProto()));
            return res;
        }

        [HttpGet("weapons")]
        public async Task<ListCharacterModelWeaponsResponsePb> ListWeapons(CancellationToken ct)
        {
            await Task.CompletedTask;
            var res = new ListCharacterModelWeaponsResponsePb();
            res.Weapons.AddRange(_cache.GetAllWeapons().Select(w => w.ToProto()));
            return res;
        }

        [HttpGet("{characterId:int}/recipe")]
        public async Task<GetCharacterVisualRecipeResponsePb> GetRecipe([FromRoute] int characterId, CancellationToken ct)
        {
            await Task.CompletedTask;
            var recipe = _cache.BuildRecipe(characterId) ?? throw new Exception("Recipe not found");
            return new GetCharacterVisualRecipeResponsePb { Recipe = recipe.ToProto() };
        }



    }
}
