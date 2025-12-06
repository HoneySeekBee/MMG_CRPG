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

        // jsonb 컬럼 식별 목록 (필요한 만큼 추가)
        private static readonly HashSet<string> JsonbColumns = new()
        {
            "Bonus",
            "Meta",
            "Tags",
            "Effect"
        };

        public SeedExporter(IDbConnection db, string outputDir)
        {
            _db = db;
            _outputDir = outputDir;
        }

        public async Task ExportAllAsync()
        {
            if (!Directory.Exists(_outputDir))
                Directory.CreateDirectory(_outputDir);

            var tables = new[]
            {
                "Battles", "Chapters",
                "CharacterExp", "CharacterModel", "CharacterModelParts", "CharacterModelWeapon",
                "CharacterPromotion", "CharacterPromotionMaterials", "CharacterSkills",
                "CharacterStatProgression", "Characters",
                "Currencies", "Element", "ElementAffinity", "EquipSlots", "Faction",
                "GachaBanner", "GachaPool", "GachaPoolEntry",
                "Icons", "Item", "ItemEffect", "ItemPrice", "ItemStat", "ItemType",
                "MonsterStatProgression", "Monsters", "Portraits", "Rarity", "Role",
                "SkillLevels", "Skills",
                "StageBatches", "StageDrops", "StageFirstClearRewards", "StageRequirements",
                "StageWaveEnemies", "StageWaves", "Stages", "StatTypes",
                "Synergy", "SynergyBonus", "SynergyRule", "SynergyTarget",
            };

            foreach (var table in tables)
            {
                var rows = await _db.QueryAsync($"SELECT * FROM \"{table}\"");
                var normalized = new List<Dictionary<string, object?>>();

                foreach (var row in rows)
                {
                    var dict = new Dictionary<string, object?>();

                    foreach (var prop in (IDictionary<string, object?>)row)
                    {
                        var key = prop.Key;
                        var value = prop.Value;

                        // 1. jsonb 컬럼이면 반드시 JsonDocument로 변환
                        if (JsonbColumns.Contains(key))
                        {
                            if (value is string jsonString)
                            {
                                dict[key] = JsonDocument.Parse(jsonString).RootElement.Clone();
                            }
                            else if (value is JsonElement elem)
                            {
                                dict[key] = JsonDocument.Parse(elem.GetRawText()).RootElement.Clone();
                            }
                            else if (value != null)
                            {
                                string serialized = JsonSerializer.Serialize(value);
                                dict[key] = JsonDocument.Parse(serialized).RootElement.Clone();
                            }
                            else
                            {
                                dict[key] = null;
                            }

                            continue;
                        }

                        // 2. string 타입이지만 JSON처럼 생긴 경우
                        if (value is string s && IsJsonString(s))
                        {
                            try
                            {
                                dict[key] = JsonDocument.Parse(s).RootElement.Clone();
                            }
                            catch
                            {
                                dict[key] = s;
                            }
                        }
                        // 3. JsonElement 객체면 그대로 저장
                        else if (value is JsonElement element)
                        {
                            dict[key] = element.Clone();
                        }
                        // 4. 일반 타입 그대로 저장
                        else
                        {
                            dict[key] = value;
                        }
                    }

                    normalized.Add(dict);
                }

                var json = JsonSerializer.Serialize(
                    normalized,
                    new JsonSerializerOptions { WriteIndented = true });

                var path = Path.Combine(_outputDir, $"{table}.json");
                await File.WriteAllTextAsync(path, json);

                Console.WriteLine($"[SeedExporter] Exported {table} → {path}");
            }
        }

        private static bool IsJsonString(string s)
        {
            s = s.Trim();
            return (s.StartsWith("{") && s.EndsWith("}")) ||
                   (s.StartsWith("[") && s.EndsWith("]"));
        }
    }
}