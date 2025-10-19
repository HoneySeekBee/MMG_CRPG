 using Application.Skills;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebServer.Options;
using WebServer.Protos;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/pb/skills")]
    [Produces("application/x-protobuf")]
    public class SkillsProtoController : ControllerBase
    {
        private readonly ISkillCache _cache;
        private readonly string _imageBase;
        private readonly string _iconsSubdir; 

        public SkillsProtoController(ISkillCache cache, IOptions<AssetsOptions> assetsOpt)
        {
            _cache = cache;
            var o = assetsOpt.Value;
            _imageBase = (o.ImageUrl ?? "").TrimEnd('/');
            _iconsSubdir = o.IconsSubdir ?? "icons"; 
        }

        // GET /api/pb/skills
        [HttpGet]
        public IActionResult GetAll()
        {
            var dtoList = _cache.GetAll();

            var resp = new SkillsResponse
            {
                Version = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            foreach (var s in dtoList)
            {
                var skill = new SkillMessage
                {
                    SkillId = s.SkillId,
                    Name = s.Name ?? string.Empty,
                    Type = (SkillTypePb)s.Type,
                    ElementId = s.ElementId,
                    IconId = s.IconId,
                    IsActive = s.IsActive,
                    TargetingType = (SkillTargetingTypePb)s.TargetingType,
                    TargetSide = (TargetSideTypePb)s.TargetSide,
                    AoeShape = (AoeShapeTypePb)s.AoeShape,
                    BaseInfo = ToStruct(s.BaseInfo),
                    IsPassive = !s.IsActive,
                    IconUrl = BuildIconUrl(s.IconId)
                };

                if (s.Tag != null && s.Tag.Length > 0)
                    skill.Tags.AddRange(s.Tag);

                if (s.Levels != null)
                {
                    foreach (var l in s.Levels.OrderBy(x => x.Level))
                    {
                        skill.Levels.Add(new SkillLevelMessage
                        {
                            SkillId = l.SkillId,
                            Level = l.Level,
                            Values = ToStruct(l.Values),
                            Description = l.Description ?? string.Empty,
                            Materials = ToStruct(l.Materials),
                            CostGold = l.CostGold,
                            IsPassive = l.IsPassive
                        });
                    }
                }

                resp.Skills.Add(skill);
            }

            return File(resp.ToByteArray(), "application/x-protobuf");
        }
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var s = _cache.GetById(id);
            if (s is null) return NotFound();

            var resp = new SkillsResponse
            {
                Version = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            var skill = new SkillMessage
            {
                SkillId = s.SkillId,
                Name = s.Name ?? string.Empty,
                Type = (SkillTypePb)s.Type,
                ElementId = s.ElementId,
                IconId = s.IconId,
                IsActive = s.IsActive,
                TargetingType = (SkillTargetingTypePb)s.TargetingType,
                TargetSide = (TargetSideTypePb)s.TargetSide,
                AoeShape = (AoeShapeTypePb)s.AoeShape,
                BaseInfo = ToStruct(s.BaseInfo),
                IsPassive = !s.IsActive,
                IconUrl = BuildIconUrl(s.IconId)
            };

            if (s.Tag != null && s.Tag.Length > 0)
                skill.Tags.AddRange(s.Tag);

            if (s.Levels != null)
            {
                foreach (var l in s.Levels.OrderBy(x => x.Level))
                {
                    skill.Levels.Add(new SkillLevelMessage
                    {
                        SkillId = l.SkillId,
                        Level = l.Level,
                        Values = ToStruct(l.Values),
                        Description = l.Description ?? string.Empty,
                        Materials = ToStruct(l.Materials),
                        CostGold = l.CostGold,
                        IsPassive = l.IsPassive
                    });
                }
            }

            resp.Skills.Add(skill);
            return File(resp.ToByteArray(), "application/x-protobuf");
        }
        private static Struct ToStruct(object? jsonb)
        {
            if (jsonb == null) return new Struct();

            switch (jsonb)
            {
                case string s when string.IsNullOrWhiteSpace(s):
                    return new Struct();
                case string s:
                    return Struct.Parser.ParseJson(s);

                case System.Text.Json.JsonDocument jd:
                    return Struct.Parser.ParseJson(jd.RootElement.GetRawText());

                default:
                    // 직렬화 가능한 객체는 JSON 문자열로 변환 후 파싱
                    var text = System.Text.Json.JsonSerializer.Serialize(jsonb);
                    return Struct.Parser.ParseJson(text);
            }
        }

        private string BuildIconUrl(int iconId)
        {
            if (string.IsNullOrEmpty(_imageBase)) return string.Empty;
            return $"{_imageBase}/{_iconsSubdir}/{iconId}.png";
        }
    }
}
