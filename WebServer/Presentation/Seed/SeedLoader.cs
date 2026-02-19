using System.Data;
using System.Text.Json;
using Dapper;
using MySqlConnector;

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

        private static readonly HashSet<string> JsonbColumns = new()
        {
            "Meta",
            "Tags",
            "Effect",
            "Bonus"
        };

        private object? Normalize(JsonElement elem)
        {
            switch (elem.ValueKind)
            {
                case JsonValueKind.String:
                    var s = elem.GetString()!;
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

        private async Task ExecuteWithParamsAsync(string sql, Dictionary<string, object?> row)
        {
            using var cmd = new MySqlCommand(sql, (MySqlConnection)_db);

            foreach (var kv in row)
            {
                string col = kv.Key;
                object? val = kv.Value;
                if (JsonbColumns.Contains(col))
                {
                    if (val is JsonDocument doc)
                    {
                        cmd.Parameters.AddWithValue($"@{col}", doc.RootElement.GetRawText());
                    }
                    else if (val is string s)
                    {
                        cmd.Parameters.AddWithValue($"@{col}", s);
                    }
                    else if (val == null)
                    {
                        cmd.Parameters.AddWithValue($"@{col}", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue($"@{col}", JsonSerializer.Serialize(val));
                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue($"@{col}", val ?? DBNull.Value);
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

                "Skills",
                "SkillLevels",

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

                var pks = await GetPrimaryKeysAsync(table);

                foreach (var row in rows)
                {
                    var normalized = row.ToDictionary(
                        kv => kv.Key,
                        kv => Normalize(kv.Value)
                    );

                    var cleaned = RemoveNulls(normalized);
                    if (!cleaned.Any())
                        continue;

                    var sql = BuildUpsertSql(table, pks, cleaned);
                    await ExecuteWithParamsAsync(sql, cleaned);
                }

                Console.WriteLine($"[SeedLoader] Loaded JSON → {table}");
            }

            Console.WriteLine("\n=== Seed Load Completed ===\n");
        }
        private async Task<List<string>> GetPrimaryKeysAsync(string table)
        {
            string sql = @"
        SELECT COLUMN_NAME
        FROM   information_schema.KEY_COLUMN_USAGE
        WHERE  TABLE_SCHEMA = DATABASE()
          AND  TABLE_NAME   = @table
          AND  CONSTRAINT_NAME = 'PRIMARY'
        ORDER BY ORDINAL_POSITION;
    ";

            var keys = await _db.QueryAsync<string>(sql, new { table });
            return keys.ToList();
        }

        private string BuildUpsertSql(string table, List<string> pks, Dictionary<string, object?> row)
        {
            var columns = row.Keys.ToList();

            string insertCols = string.Join(",", columns.Select(c => $"`{c}`"));
            string insertVals = string.Join(",", columns.Select(c => $"@{c}"));

            var updateCols = columns
                .Where(c => !pks.Contains(c))
                .Select(c => $"`{c}` = VALUES(`{c}`)");

            string updateSql = updateCols.Any()
                ? "ON DUPLICATE KEY UPDATE " + string.Join(",", updateCols)
                : "ON DUPLICATE KEY UPDATE " + $"`{pks.First()}` = `{pks.First()}`";

            return $@"
        INSERT INTO `{table}` ({insertCols})
        VALUES ({insertVals})
        {updateSql};
    ";
        }

    }
}
