using Domain.Entities;
using Domain.Entities.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public static class SeedData
    {
        public static void EnsureSeeded(GameDBContext db)
        {
            db.Database.EnsureCreated();

            // ---------- 1) Elements (전용 생성자 사용) ----------
            UpsertElement(db, key: "fire", label: "Fire", color: "#FF5A36", sort: 0);
            UpsertElement(db, key: "water", label: "Water", color: "#3BA7FF", sort: 1);

            // ---------- 2) Rarity / Role / Faction (public setter OK) ----------
            UpsertRarity(db, key: "r", label: "R", stars: 1, sort: 0, color: "#BBBBBB");
            UpsertRarity(db, key: "sr", label: "SR", stars: 2, sort: 1, color: "#FFD54F");

            UpsertRole(db, key: "attacker", label: "Attacker", sort: 0);
            UpsertRole(db, key: "defender", label: "Defender", sort: 1);

            UpsertFaction(db, key: "neutral", label: "Neutral", sort: 0);

            db.SaveChanges(); // ↑ 선행 테이블 확정

            // 키/아이디 가져오기 (존재 보장됨)
            var fireId = db.Elements.Where(e => e.Key == "fire").Select(e => e.ElementId).Single();
            var waterId = db.Elements.Where(e => e.Key == "water").Select(e => e.ElementId).Single();
            var rarityR = db.Set<Rarity>().Where(x => x.Key == "r").Select(x => x.RarityId).Single();
            var raritySR = db.Set<Rarity>().Where(x => x.Key == "sr").Select(x => x.RarityId).Single();
            var roleAtk = db.Set<Role>().Where(x => x.Key == "attacker").Select(x => x.RoleId).Single();
            var roleDef = db.Set<Role>().Where(x => x.Key == "defender").Select(x => x.RoleId).Single();
            var facNeutral = db.Set<Faction>().Where(x => x.Key == "neutral").Select(x => x.FactionId).Single();

            // ---------- 3) Characters (팩토리 사용) ----------
            if (!db.Characters.Any())
            {
                var c1 = Character.Create("Agnis", raritySR, facNeutral, roleAtk, fireId, 1);
                var c2 = Character.Create("Marin", rarityR, facNeutral, roleDef, waterId, 1);
                db.Characters.AddRange(c1, c2);
                db.SaveChanges();
            }

            // ---------- 4) GachaPool (팩토리 + 엔트리) ----------
            if (!db.Set<GachaPool>().Any())
            {
                var agnisId = db.Characters.Single(c => c.Name == "Agnis").Id;
                var marinId = db.Characters.Single(c => c.Name == "Marin").Id;

                var entries = new List<GachaPoolEntry>
            {
                GachaPoolEntry.Create(agnisId, grade: 2, rateUp: true,  weight: 10),
                GachaPoolEntry.Create(marinId, grade: 1, rateUp: false, weight: 90),
            };

                var pool = GachaPool.Create(
                    name: "Starter",
                    scheduleStart: DateTimeOffset.UtcNow,
                    scheduleEnd: null,
                    pityJson: null,
                    tablesVersion: "v1",
                    configJson: "{}",
                    entries: entries
                );

                db.Add(pool);
                db.SaveChanges();
            }
        }

        // ---------------- helpers (업서트) ----------------
        static void UpsertElement(GameDBContext db, string key, string label, string color, short sort)
        {
            var e = db.Elements.SingleOrDefault(x => x.Key == key);
            if (e == null)
            {
                e = new Element(key, label, color, sort, iconId: null, metaJson: "{}");
                db.Elements.Add(e);
            }
            else
            {
                // 필요하면 e.Update(label, color, sort, iconId: null, metaJson: "{}");
            }
        }

        static void UpsertRarity(GameDBContext db, string key, string label, short stars, short sort, string? color)
        {
            var r = db.Set<Rarity>().SingleOrDefault(x => x.Key == key);
            if (r == null)
            {
                r = new Rarity { Key = key, Label = label, Stars = stars, SortOrder = sort, ColorHex = color, IsActive = true };
                db.Add(r);
            }
            else
            {
                r.Label = label; r.Stars = stars; r.SortOrder = sort; r.ColorHex = color; r.IsActive = true;
            }
        }

        static void UpsertRole(GameDBContext db, string key, string label, short sort)
        {
            var role = db.Set<Role>().SingleOrDefault(x => x.Key == key);
            if (role == null) db.Add(new Role { Key = key, Label = label, SortOrder = sort, IsActive = true });
            else { role.Label = label; role.SortOrder = sort; role.IsActive = true; }
        }

        static void UpsertFaction(GameDBContext db, string key, string label, short sort)
        {
            var f = db.Set<Faction>().SingleOrDefault(x => x.Key == key);
            if (f == null) db.Add(new Faction { Key = key, Label = label, SortOrder = sort, IsActive = true });
            else { f.Label = label; f.SortOrder = sort; f.IsActive = true; }
        }
    }
}
