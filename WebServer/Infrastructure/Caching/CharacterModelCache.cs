using Application.CharacterModels;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Caching
{
    public sealed class CharacterModelCache : ICharacterModelCache
    {
        private readonly NpgsqlDataSource _dataSource; 

        // 원자적 스왑을 위한 필드
        private Dictionary<int, CharacterModelDto> _modelsById = new();
        private Dictionary<int, CharacterModelPartDto> _partsById = new();
        private Dictionary<int, CharacterModelWeaponDto> _weaponsById = new();
        private Dictionary<string, int> _weaponCodeToId = new(); // 선택: code→id 역참조

        public CharacterModelCache(NpgsqlDataSource dataSource)
            => _dataSource = dataSource;


        public CharacterModelDto? GetModel(int characterId)
            => _modelsById.TryGetValue(characterId, out var v) ? v : null;

        public CharacterModelPartDto? GetPart(int partId)
            => _partsById.TryGetValue(partId, out var v) ? v : null;

        public CharacterModelWeaponDto? GetWeapon(int weaponId)
            => _weaponsById.TryGetValue(weaponId, out var v) ? v : null;
        public IEnumerable<CharacterModelDto> GetAllModels() => _modelsById.Values;
        public IEnumerable<CharacterModelPartDto> GetAllParts() => _partsById.Values;
        public IEnumerable<CharacterModelWeaponDto> GetAllWeapons() => _weaponsById.Values;

        public async Task ReloadAsync(CancellationToken ct = default)
        {
            var opts = new DbContextOptionsBuilder<GameDBContext>()
            .UseNpgsql(_dataSource)
            .Options;

            await using var db = new GameDBContext(opts);
            var conn = (NpgsqlConnection)db.Database.GetDbConnection();
            Console.WriteLine($"[Ctx] DS Hash = {conn.DataSource?.GetHashCode()}");

            // Models
            var models = await db.Set<Domain.Entities.Characters.CharacterModel>()
                .AsNoTracking()
                .Select(m => new CharacterModelDto
                {
                    CharacterId = m.CharacterId,
                    BodyType =  m.BodyType.ToString(),           // enum -> string
                    AnimationType = m.AnimationType.ToString(),      // enum -> string
                    WeaponLId = m.WeaponLId,
                    WeaponRId = m.WeaponRId,
                    PartHeadId = m.PartHeadId,
                    PartHairId = m.PartHairId,
                    PartMouthId = m.PartMouthId,
                    PartEyeId = m.PartEyeId,
                    PartAccId = m.PartAccId
                })
                .ToListAsync(ct);
            Console.WriteLine($"[Reload] Models fetched: {models.Count}  ");

            // Parts
            var parts = await db.Set<Domain.Entities.Characters.CharacterModelPart>()
                .AsNoTracking()
                .Select(p => new CharacterModelPartDto
                {
                    PartId = p.PartId,
                    PartKey = p.PartKey,
                    PartType = p.PartType.ToString(),                // enum -> string 
                })
                .ToListAsync(ct);
            Console.WriteLine($"[Reload] Parts fetched:   {parts.Count} v");

            // Weapons
            var weapons = await db.Set<Domain.Entities.Characters.CharacterModelWeapon>()
                .AsNoTracking()
                .Select(w => new CharacterModelWeaponDto
                {
                    WeaponId = w.WeaponId,
                    Code = w.Code,
                    DisplayName = w.DisplayName,
                    IsTwoHanded = w.IsTwoHanded
                })
                .ToListAsync(ct);
            Console.WriteLine($"[Reload] Weapons fetched: {weapons.Count}  s");

            // Build dictionaries
            var modelsById = models.ToDictionary(x => x.CharacterId);
            var partsById = parts.ToDictionary(x => x.PartId);
            var weaponsById = weapons.ToDictionary(x => x.WeaponId);
            var weaponCodeToId = weapons.ToDictionary(x => x.Code, x => x.WeaponId, StringComparer.OrdinalIgnoreCase);

            // Atomic swap
            _modelsById = modelsById;
            _partsById = partsById;
            _weaponsById = weaponsById;
            _weaponCodeToId = weaponCodeToId;
             
        }
        public CharacterVisualRecipe? BuildRecipe(int characterId)
        {
            if (!_modelsById.TryGetValue(characterId, out var m))
                return null;

            string? HeadKey = m.PartHeadId is int ph && _partsById.TryGetValue(ph, out var p1) ? p1.PartKey : null;
            string? HairKey = m.PartHairId is int ph2 && _partsById.TryGetValue(ph2, out var p2) ? p2.PartKey : null;
            string? MouthKey = m.PartMouthId is int pm && _partsById.TryGetValue(pm, out var p3) ? p3.PartKey : null;
            string? EyeKey = m.PartEyeId is int pe && _partsById.TryGetValue(pe, out var p4) ? p4.PartKey : null;
            string? AccKey = m.PartAccId is int pa && _partsById.TryGetValue(pa, out var p5) ? p5.PartKey : null;

            // 무기 Addressable 키가 필요하면 CharacterModelWeapon에 컬럼(예: prefab_key)을 추가해 사용하는 걸 추천
            string? WeaponLKey = null;
            if (m.WeaponLId is int wl && _weaponsById.TryGetValue(wl, out var wlMeta))
                WeaponLKey = WeaponKeyFromCode(wlMeta.Code); // 규칙 또는 별도 컬럼

            string? WeaponRKey = null;
            if (m.WeaponRId is int wr && _weaponsById.TryGetValue(wr, out var wrMeta))
                WeaponRKey = WeaponKeyFromCode(wrMeta.Code);

            return new CharacterVisualRecipe
            {
                CharacterId = m.CharacterId,
                BodyType = m.BodyType.ToString(),
                AnimationType = m.AnimationType.ToString(),
                WeaponLKey = WeaponLKey,
                WeaponRKey = WeaponRKey,
                HeadKey = HeadKey,
                HairKey = HairKey,
                MouthKey = MouthKey,
                EyeKey = EyeKey,
                AccKey = AccKey
            };
        }

        private static string WeaponKeyFromCode(string code)
            => $"weapons/{code}".ToLowerInvariant();
    }
}
