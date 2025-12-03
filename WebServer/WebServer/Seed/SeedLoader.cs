using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using NpgsqlTypes;

namespace WebServer.Seed
{
    public class SeedLoader
    {
        private readonly IDbConnection _db;
        private readonly string _seedDir;

        public SeedLoader(IDbConnection db, string seedDir)
        {
            _db = db;
            _seedDir = seedDir;
        }
        private static readonly Dictionary<string, string> EnumColumnTypes = new()
        {
            { "body_type", "BodySize" },
            { "animation_type", "CharacterAnimationType" },
            { "part_type", "PartType" }
        }; 
        private static readonly HashSet<string> JsonbColumns = new()
{
    "Meta",
    "Tags",
    "Effect"
};

        private object? Normalize(JsonElement elem)
        {
            switch (elem.ValueKind)
            {
                case JsonValueKind.String:
                    var s = elem.GetString()!;
                    // 문자열이 JSON 객체/배열처럼 보이면 JsonDocument 로 변환
                    if ((s.StartsWith("{") && s.EndsWith("}")) ||
                        (s.StartsWith("[") && s.EndsWith("]")))
                    {
                        try { return JsonDocument.Parse(s); }
                        catch { Console.WriteLine("[Normalize] 실패"); }
                    }
                    if (DateTime.TryParse(s, out var dt))
                        return dt;
                    return s;

                case JsonValueKind.Number:
                    if (elem.TryGetInt64(out long l)) return l;
                    if (elem.TryGetDouble(out double d)) return d;
                    return elem.ToString();

                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;

                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    return JsonDocument.Parse(elem.GetRawText());

                default:
                    return elem.ToString();
            }
        }
        private Dictionary<string, object?> RemoveNulls(Dictionary<string, object?> dict)
        {
            return dict
                .Where(kv => kv.Value != null && kv.Value != DBNull.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
        private string BuildSql(string table, Dictionary<string, object?> row)
        {
            var colParts = new List<string>();
            var valParts = new List<string>();

            foreach (var kv in row)
            {
                string col = kv.Key;
                colParts.Add($"\"{col}\"");

                if (EnumColumnTypes.TryGetValue(col, out var enumType))
                    valParts.Add($"@{col}::\"{enumType}\"");
                else
                    valParts.Add($"@{col}");
            }

            return $@"
                INSERT INTO ""{table}"" ({string.Join(",", colParts)})
                VALUES ({string.Join(",", valParts)})
                ON CONFLICT DO NOTHING;
            ";
        }
        private async Task ExecuteWithParamsAsync(string sql, Dictionary<string, object?> row)
        {
            using var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)_db);

            foreach (var kv in row)
            {
                string col = kv.Key;
                object? val = kv.Value;
                if (JsonbColumns.Contains(col))
                {
                    if (val is JsonDocument doc)
                    {
                        cmd.Parameters.Add(new NpgsqlParameter(col, NpgsqlDbType.Jsonb)
                        {
                            Value = doc.RootElement.GetRawText()
                        });
                    }
                    else if (val is string s)
                    {
                        // 문자열이지만 JSON literal "{}" 또는 "[]" 같은 형태인 경우 jsonb로 직접 캐스팅
                        cmd.Parameters.Add(new NpgsqlParameter(col, NpgsqlDbType.Jsonb)
                        {
                            Value = s
                        });
                    }
                    else if (val == null)
                    {
                        cmd.Parameters.Add(new NpgsqlParameter(col, NpgsqlDbType.Jsonb)
                        {
                            Value = DBNull.Value
                        });
                    }
                    else
                    {
                        cmd.Parameters.Add(new NpgsqlParameter(col, NpgsqlDbType.Jsonb)
                        {
                            Value = JsonSerializer.Serialize(val)
                        });
                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue(col, val ?? DBNull.Value);
                }
            }

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task LoadAllAsync()
        {
            if (!Directory.Exists(_seedDir))
            {
                Console.WriteLine("[SeedLoader] No DataSeeds folder found.");
                return;
            }

            var files = Directory.GetFiles(_seedDir, "*.json");
            var loadOrder = new List<string>
            {
                "Icons",
                "Portraits",

                "Element",
                "ElementAffinity",
                "Faction",
                "EquipSlots",
                "Currencies",
                "Rarity",
                "Role",

                "StatTypes",
                "ItemType",

                "Synergy",
                "SynergyBonus",
                "SynergyRule",
                "SynergyTarget",

                "Characters",
                "CharacterExp",
                "CharacterModel",
                "CharacterModelParts",
                "CharacterModelWeapon",
                "CharacterPromotion",
                "CharacterPromotionMaterials",
                "CharacterStatProgression",
                "CharacterSkills",  

                "Item",
                "ItemStat",
                "ItemEffect",
                "ItemPrice",


                "Monsters",
                "MonsterStatProgression",

                "Battles",
                "Chapters",
                "Stages",
                "StageBatches",
                "StageDrops",
                "StageFirstClearRewards",
                "StageRequirements",
                "StageWaves",
                "StageWaveEnemies",

                "GachaBanner",
                "GachaPool",
                "GachaPoolEntry",


            };
            files = files
      .OrderBy(f =>
      {
          var name = Path.GetFileNameWithoutExtension(f);
          var idx = loadOrder.FindIndex(x =>
              string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
          if (idx < 0)
              Console.WriteLine($"[WARN] File {name} is not in loadOrder!");
          return idx < 0 ? 999 : idx;
      })
      .ThenBy(f => f)
      .ToArray();

            foreach (var file in files)
            {
                var table = Path.GetFileNameWithoutExtension(file);
                var json = await File.ReadAllTextAsync(file);

                var rows =
                    JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json)!;

                foreach (var row in rows)
                {
                    var normalized = row.ToDictionary(
                        kv => kv.Key,
                        kv => Normalize(kv.Value)
                    );

                    var cleaned = RemoveNulls(normalized);

                    if (!cleaned.Any())
                        continue;

                    var sql = BuildSql(table, cleaned);
                    await ExecuteWithParamsAsync(sql, cleaned);
                }

                Console.WriteLine($"[SeedLoader] Loaded JSON → {table}");
            }

            Console.WriteLine("\n=== Seed Load Completed ===\n");
        } 
    }
}
