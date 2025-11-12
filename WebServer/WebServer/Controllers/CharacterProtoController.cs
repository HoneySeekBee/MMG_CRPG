using Application.Character; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebServer.Options;
using WebServer.Protos; 
using Google.Protobuf.WellKnownTypes;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/characters")]
    [Produces("application/x-protobuf")]
    public class CharacterProtoController : ControllerBase
    {
        private readonly ICharacterCache _cache; 
        private readonly string _imageBase; 
        private readonly string _iconsSubdir;
        private readonly string _portraitsSubdir;

        public CharacterProtoController(ICharacterCache cache, IOptions<AssetsOptions> assetsOpt)
        {
            _cache = cache;
            var o = assetsOpt.Value;
            _imageBase = (o.ImageUrl ?? "").TrimEnd('/');
            _iconsSubdir = o.IconsSubdir ?? "icons";
            _portraitsSubdir = o.PortraitsSubdir ?? "portraits";
        }

        [HttpGet]
        public async Task<ActionResult<CharactersResponsePb>> GetAllAsync(CancellationToken ct)
        {
            // 캐시 불러오기 (이미 메모리에 있으므로 await 불필요하지만 인터페이스 통일을 위해 Task 사용)
            var list = _cache.GetAll();

            var result = new CharactersResponsePb
            {
                Version = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            foreach (var dto in list)
            {
                var pb = new CharacterDetailPb
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    RarityId = dto.RarityId,
                    ElementId = dto.ElementId,
                    RoleId = dto.RoleId,
                    FactionId = dto.FactionId,
                    IsLimited = dto.IsLimited,
                    IconId = dto.IconId.HasValue ? dto.IconId.Value : null,
                    PortraitId = dto.PortraitId.HasValue ? dto.PortraitId.Value : null,
                    ReleaseDate = dto.ReleaseDate.HasValue
                        ? Timestamp.FromDateTime(dto.ReleaseDate.Value.UtcDateTime)
                        : null,
                    IconUrl = dto.IconId.HasValue ? $"{_imageBase}/{_iconsSubdir}/{dto.IconId.Value}.png" : "",
                    PortraitUrl = dto.PortraitId.HasValue ? $"{_imageBase}/{_portraitsSubdir}/{dto.PortraitId.Value}.png" : "",
                    FormationNum = dto.formationNum
                };

                // Tags
                pb.Tags.AddRange(dto.Tags);

                // MetaJson → Struct
                if (!string.IsNullOrEmpty(dto.MetaJson))
                {
                    pb.Meta = Struct.Parser.ParseJson(dto.MetaJson);
                }

                // Skills
                foreach (var skill in dto.Skills)
                {
                    pb.Skills.Add(new CharacterSkillPb
                    {
                        Slot = (SkillSlotPb)skill.Slot,
                        SkillId = skill.SkillId,
                        UnlockTier = skill.UnlockTier,
                        UnlockLevel = skill.UnlockLevel
                    });
                }

                // Stat Progression
                foreach (var s in dto.StatProgressions)
                {
                    pb.StatProgressions.Add(new CharacterStatProgressionPb
                    {
                        Level = s.Level,
                        Hp = s.HP,
                        Atk = s.ATK,
                        Def = s.DEF,
                        Spd = s.SPD,
                        CritRate = (double)s.CriRate,
                        CritDamage = (double)s.CriDamage
                    });
                }

                // Promotions
                foreach (var promo in dto.Promotions)
                {
                    var promoPb = new CharacterPromotionPb
                    {
                        Tier = promo.Tier,
                        MaxLevel = promo.MaxLevel,
                        CostGold = promo.CostGold
                    };

                    // Bonus
                    if (promo.Bonus != null)
                    {
                        promoPb.Bonus = new StatModifierPb
                        {
                            Hp = promo.Bonus.HP.HasValue ?   promo.Bonus.HP.Value : null,
                            Atk = promo.Bonus.ATK.HasValue ? promo.Bonus.ATK.Value : null,
                            Def = promo.Bonus.DEF.HasValue ? promo.Bonus.DEF.Value : null,
                            Spd = promo.Bonus.SPD.HasValue ? promo.Bonus.SPD.Value : null,
                            CritRate = promo.Bonus.CritRate.HasValue ? (double)promo.Bonus.CritRate.Value : null,
                            CritDamage = promo.Bonus.CritDamage.HasValue ? (double)promo.Bonus.CritDamage.Value : null
                        };
                    }

                    // Materials
                    foreach (var mat in promo.Materials)
                    {
                        promoPb.Materials.Add(new PromotionMaterialPb
                        {
                            ItemId = mat.ItemId,
                            Quantity = mat.Quantity
                        });
                    }

                    pb.Promotions.Add(promoPb);
                }

                result.Characters.Add(pb);
            }

            return Ok(result);
        }
    }
}
