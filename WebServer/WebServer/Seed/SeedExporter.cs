using System.Data;
using System.Text.Json;
using Dapper;

namespace WebServer.Seed
{
    // dotnet run -- export-seeds

    public class SeedExporter
    {
        private readonly IDbConnection _db;
        private readonly string _outputDir;

        public SeedExporter(IDbConnection db, string outputDir)
        {
            _db = db;
            _outputDir = outputDir;
        }

        public async Task ExportAllAsync()
        {
            if (!Directory.Exists(_outputDir))
                Directory.CreateDirectory(_outputDir);

            // 필요한 테이블 목록 (여기만 수정하면 됨)
            var tables = new[]
              { 
                "Battles",
                "Chapters",
                "CharacterExp",
                "CharacterModel",
                "CharacterModelParts",
                "CharacterModelWeapon",
                "CharacterPromotion",
                "CharacterPromotionMaterials",
                "CharacterSkills",
                "CharacterStatProgression",
                "Characters",
                "Currencies",
                "Element",
                "ElementAffinity",
                "EquipSlots",
                "Faction",
                "GachaBanner", 
                "GachaPool",
                "GachaPoolEntry",
                "Icons",
                "Item",
                "ItemEffect",
                "ItemPrice",
                "ItemStat",
                "ItemType",
                "MonsterStatProgression",
                "Monsters",
                "Portraits",
                "Rarity",
                "Role",  
                "SkillLevels",
                "Skills",
                "StageBatches",
                "StageDrops",
                "StageFirstClearRewards",
                "StageRequirements",
                "StageWaveEnemies",
                "StageWaves",
                "Stages",
                "StatTypes",
                "Synergy",
                "SynergyBonus",
                "SynergyRule",
                "SynergyTarget",     
            };
            foreach (var table in tables)
            {
                var rows = await _db.QueryAsync($"SELECT * FROM \"{table}\"");

                var json = JsonSerializer.Serialize(rows,
                    new JsonSerializerOptions { WriteIndented = true });

                var path = Path.Combine(_outputDir, $"{table}.json");
                await File.WriteAllTextAsync(path, json);

                Console.WriteLine($"[SeedExporter] Exported {table} → {path}");
            }

            Console.WriteLine("\n=== Seed Export Completed ===\n");
        }
    }
}
