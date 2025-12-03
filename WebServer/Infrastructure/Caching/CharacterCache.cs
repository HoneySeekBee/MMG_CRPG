using Application.Character;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore; 

namespace Infrastructure.Caching
{
    public class CharacterCache : ICharacterCache
    {
        private readonly IDbContextFactory<GameDBContext> _factory;
        public CharacterCache(IDbContextFactory<GameDBContext> factory) => _factory = factory;
        private List<CharacterDetailDto> _cache = new();
        private Dictionary<int, CharacterDetailDto> _byId = new();

        public IReadOnlyList<CharacterDetailDto> GetAll() => _cache;
        public CharacterDetailDto? GetById(int id) => _byId.TryGetValue(id, out var v) ? v : null;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            await using var db = await _factory.CreateDbContextAsync(ct);

            var list = await db.Characters
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new CharacterDetailDto( 
                    c.Id,
                    c.Name,
                    c.RarityId,
                    c.ElementId,
                    c.RoleId,
                    c.FactionId,
                    c.IconId,
                    c.PortraitId,
                    c.IsLimited,
                    c.ReleaseDate,
                    c.FormationNumber,
                    (IReadOnlyList<string>)(c.Tags ?? Array.Empty<string>()),
                    c.MetaJson,

                    // Skills
                    c.CharacterSkills
                        .OrderBy(cs => cs.Slot)
                        .ThenBy(cs => cs.UnlockTier)
                        .ThenBy(cs => cs.UnlockLevel)
                        .Select(cs => new CharacterSkillDto( 
                            cs.Slot,
                            cs.SkillId,
                            cs.UnlockTier,
                            cs.UnlockLevel
                        ))
                        .ToList(),

                    // Stat progressions
                    c.CharacterStatProgressions
                        .OrderBy(sp => sp.Level)
                        .Select(sp => new CharacterStatProgressionDto( 
                            sp.Level,
                            sp.HP,
                            sp.ATK,
                            sp.DEF,
                            sp.SPD,
                            sp.CriRate,     // 엔티티: CriRate/CriDamage ↔ DTO: CritRate/CritDamage (포지션만 맞추면 OK)
                            sp.CriDamage,
                            sp.Range
                        ))
                        .ToList(),

                    // Promotions (+ materials, bonus)
                    c.CharacterPromotions
                        .OrderBy(p => p.Tier)
                        .Select(p => new CharacterPromotionDto( 
                            p.Tier,
                            p.MaxLevel,
                            p.CostGold,
                            p.Bonus == null
                                ? null
                                : new StatModifierDto(
                                    // StatModifierDto(int? HP, int? ATK, int? DEF, int? SPD, decimal? CritRate, decimal? CritDamage)
                                    p.Bonus.HP,
                                    p.Bonus.ATK,
                                    p.Bonus.DEF,
                                    p.Bonus.SPD,
                                    p.Bonus.CritRate,
                                    p.Bonus.CritDamage
                                  ),
                            p.Materials
                                .OrderBy(m => m.ItemId)
                                .Select(m => new PromotionMaterialDto(
                                    // PromotionMaterialDto(int ItemId, int Quantity)
                                    (int)m.ItemId,
                                    m.Count
                                ))
                                .ToList()
                        ))
                        .ToList()
                )) 
                .ToListAsync(ct);

            // 원자적 스왑
            _cache = list;
            Console.WriteLine($"Characeter Cache {list.Count}");
            _byId = list.ToDictionary(x => x.Id);
        }
    }
}
