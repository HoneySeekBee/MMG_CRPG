using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection.Emit;
using System.Text.Json.Nodes;

namespace Infrastructure.Persistence
{
    public class GameDBContext : DbContext
    {
        public GameDBContext(DbContextOptions<GameDBContext> options) : base(options) { }

        public DbSet<Icon> Icons => Set<Icon>();
        public DbSet<Element> Elements => Set<Element>();
        public DbSet<ElementAffinity> ElementAffinities => Set<ElementAffinity>();

        public DbSet<Faction> Factions => Set<Faction>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Rarity> Rarities => Set<Rarity>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<Portrait> Portraits => Set<Portrait>();
        public DbSet<SkillLevel> SkillLevels => Set<SkillLevel>();
        public DbSet<Character> Characters => Set<Character>();
        public DbSet<CharacterSkill> CharacterSkills => Set<CharacterSkill>();
        public DbSet<CharacterStatProgression> CharacterStatProgressions => Set<CharacterStatProgression>();
        public DbSet<CharacterPromotion> CharacterPromotions => Set<CharacterPromotion>();
        public DbSet<CombatRecord> Combats => Set<CombatRecord>();
        public DbSet<CombatLogRecord> CombatLogs => Set<CombatLogRecord>();

        public DbSet<Item> Items => Set<Item>();
        public DbSet<ItemStat> ItemStats => Set<ItemStat>();
        public DbSet<ItemEffect> ItemEffects => Set<ItemEffect>();
        public DbSet<ItemPrice> ItemPrices => Set<ItemPrice>();

        public DbSet<StatType> StatTypes => Set<StatType>();

        public DbSet<ItemType> ItemTypes => Set<ItemType>();
        public DbSet<EquipSlot> EquipSlots => Set<EquipSlot>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<GachaBanner> GachaBanners => Set<GachaBanner>();

        public DbSet<GachaPool> GachaPools => Set<GachaPool>();
        public DbSet<GachaPoolEntry> GachaPoolEntries => Set<GachaPoolEntry>();

        public DbSet<Synergy> Synergies => Set<Synergy>();
        public DbSet<SynergyRule> SynergyRules => Set<SynergyRule>();
        public DbSet<SynergyBonus> SynergyBonuses => Set<SynergyBonus>();
        //public DbSet<SynergyTarget> SynergyTargets => Set<SynergyTarget>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Console.WriteLine("OnModelCreateing");
            Modeling_Icon(modelBuilder);
            Modeling_Portrait(modelBuilder);
            Modeling_Element(modelBuilder);
            Modeling_ElementAffinity(modelBuilder);
            Modeling_Faction(modelBuilder);
            Modeling_Role(modelBuilder);
            Modeling_Rarity(modelBuilder);
            Modeling_Skill(modelBuilder);
            Modeling_SkillLevel(modelBuilder);

            modelBuilder.Ignore<StatModifier>();
            modelBuilder.Ignore<PromotionMaterial>();

            Modeling_Character(modelBuilder);
            Modeling_CharacterSkill(modelBuilder);
            Modeling_CharacterStatProgression(modelBuilder);
            Modeling_CharacterPromotion(modelBuilder);

            Modeling_Combat(modelBuilder);
            Modeling_CombatLog(modelBuilder);


            Modeling_Item(modelBuilder);
            Modeling_ItemStat(modelBuilder);
            Modeling_ItemEffect(modelBuilder);
            Modeling_ItemPrice(modelBuilder);

            Modeling_StatTypes(modelBuilder);
            Modeling_ItemType(modelBuilder);
            Modeling_EquipSlot(modelBuilder);
            Modeling_Currency(modelBuilder);
            Modeling_GatchaBanner(modelBuilder);
            Modeling_GachaPool(modelBuilder);

            Modeling_Synergy(modelBuilder);
        }

        private void Modeling_Icon(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Icon>(e =>
            {
                e.ToTable("Icons"); // 테이블 명
                e.HasKey(x => x.IconId);
                e.Property(x => x.Key).IsRequired();
                e.HasIndex(x => x.Key).IsUnique();
            });
        }
        private void Modeling_Portrait(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Portrait>(e =>
            {
                e.ToTable("Portraits");                 // 테이블명
                e.HasKey(x => x.PortraitId);
                e.Property(x => x.PortraitId).ValueGeneratedOnAdd();

                e.Property(x => x.Key).IsRequired();    // 파일 키(유니크 권장)
                e.HasIndex(x => x.Key).IsUnique();

                // 스프라이트 좌표/아틀라스(없으면 NULL 허용)
                e.Property(x => x.Atlas).IsRequired(false);
                e.Property(x => x.X).IsRequired(false);
                e.Property(x => x.Y).IsRequired(false);
                e.Property(x => x.W).IsRequired(false);
                e.Property(x => x.H).IsRequired(false);

                // 캐시 무효화를 위한 버전
                e.Property(x => x.Version).HasDefaultValue(1);
            });
        }
        private void Modeling_Element(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Element>(e =>
            {
                e.ToTable("Element");
                e.HasKey(x => x.ElementId);
                e.Property(x => x.Key).IsRequired();
                e.Property(x => x.Label).IsRequired();
                e.Property(x => x.ColorHex).IsRequired();
                e.Property(x => x.SortOrder).HasColumnType("smallint");
                e.Property(x => x.Meta).HasColumnType("jsonb"); // PostgreSQL
                e.HasIndex(x => new { x.IsActive, x.SortOrder });
                e.HasIndex(x => x.Key).IsUnique();
            });
        }
        public void Modeling_ElementAffinity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ElementAffinity>(e =>
            {
                e.ToTable("ElementAffinity");
                e.HasKey(x => new { x.AttackerElementId, x.DefenderElementId });
                e.Property(x => x.Multiplier)
                .HasColumnType("numeric(4,2)")
                .HasDefaultValue(1.00m);
            });
        }

        public void Modeling_Faction(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Faction>(e =>
            {
                e.ToTable("Faction");
                e.HasKey(x => x.FactionId);
                e.Property(x => x.FactionId).ValueGeneratedOnAdd();
                e.Property(x => x.Key).IsRequired();
                e.Property(x => x.Label).IsRequired();
                e.Property(x => x.ColorHex);
                e.Property(x => x.Meta).HasColumnType("jsonb");      // pg jsonb
                e.Property(x => x.IsActive).HasDefaultValue(true);

                e.HasIndex(x => x.Key).IsUnique();
            });
        }
        public void Modeling_Role(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("Role");
                e.HasKey(x => x.RoleId);
                e.Property(x => x.RoleId).ValueGeneratedOnAdd();
                e.Property(x => x.Key).IsRequired();
                e.Property(x => x.Label).IsRequired();
                e.Property(x => x.ColorHex);
                e.Property(x => x.Meta).HasColumnType("jsonb");
                e.Property(x => x.IsActive).HasDefaultValue(true);

                e.HasIndex(x => x.Key).IsUnique();
            });
        }

        public void Modeling_Rarity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rarity>(e =>
            {
                e.ToTable("Rarity");
                e.HasKey(x => x.RarityId);
                e.Property(x => x.RarityId).ValueGeneratedOnAdd();
                e.Property(x => x.Stars).IsRequired();
                e.Property(x => x.Key).IsRequired();
                e.Property(x => x.Label).IsRequired();
                e.Property(x => x.ColorHex);
                e.Property(x => x.Meta).HasColumnType("jsonb");
                e.Property(x => x.IsActive).HasDefaultValue(true);

                e.HasIndex(x => x.Key).IsUnique();
            });
        }
        public void Modeling_Skill(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Skill>(e =>
            {
                e.ToTable("Skills");
                e.HasKey(x => x.SkillId);
                e.Property(x => x.SkillId).ValueGeneratedOnAdd();

                // 기본
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.Property(x => x.Type).HasConversion<short>().IsRequired();

                e.Property(x => x.ElementId).IsRequired();
                e.Property(x => x.IconId).IsRequired();

                e.Property(x => x.IsActive).IsRequired();

                e.Property(x => x.TargetingType).HasConversion<short>().IsRequired();
                e.Property(x => x.AoeShape).HasConversion<short>().IsRequired();
                e.Property(x => x.TargetSide).HasConversion<short>().IsRequired();

                e.Property(x => x.BaseInfo)
           .HasColumnType("jsonb")
           .IsRequired(false);

                e.Property(x => x.Tag)
           .HasColumnType("text[]")
           .HasDefaultValueSql("'{}'::text[]")
           .IsRequired();

                // 프로퍼티 기반으로 관계를 정의
                e.HasMany(s => s.Levels)
                 .WithOne()
                 .HasForeignKey(x => x.SkillId)
                 .OnDelete(DeleteBehavior.Cascade);

                var nav = e.Metadata.FindNavigation(nameof(Skill.Levels));
                nav!.SetField("_levels");
                nav.SetPropertyAccessMode(PropertyAccessMode.Field);
                e.HasIndex(x => x.Name);                 // 이름 검색 용
                e.HasIndex(x => x.Type);                 // 타입 필터
                e.HasIndex(x => x.ElementId);            // 속성 필터
                e.HasIndex(x => x.IsActive);             // 활성/비활성 필터
                e.HasIndex(x => x.TargetingType);
                e.HasIndex(x => x.AoeShape);
                e.HasIndex(x => x.TargetSide);


            });
        }
        public void Modeling_SkillLevel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SkillLevel>(e =>
            {
                e.ToTable("SkillLevels");

                // 복합 PK (SkillId, Level)
                e.HasKey(x => new { x.SkillId, x.Level });

                e.Property(x => x.Description);

                e.Property(x => x.CostGold)
                    .IsRequired();

                // Values (jsonb)
                e.Property(x => x.Values)
                    .HasColumnType("jsonb");

                // Materials (jsonb)
                e.Property(x => x.Materials)
                    .HasColumnType("jsonb");
            });
        }
        public void Modeling_Character(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Character>(e =>
            {
                e.ToTable("Characters");

                e.HasKey(x => x.Id);
                e.Property(x => x.Id)
                    .HasColumnName("CharacterId")
                    .ValueGeneratedOnAdd();

                // 기본
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.Property(x => x.RarityId).IsRequired();
                e.Property(x => x.FactionId).IsRequired();
                e.Property(x => x.RoleId).IsRequired();
                e.Property(x => x.ElementId).IsRequired();

                // 선택
                e.Property(x => x.IconId).IsRequired(false);
                e.Property(x => x.PortraitId).IsRequired(false);

                var utcConverter = new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
                    v => v.HasValue ? v.Value.ToUniversalTime() : v,  // Save: UTC로
                    v => v                                            // Read: 그대로(이미 UTC)
                );
                e.Property(x => x.ReleaseDate)
       .HasConversion(utcConverter)
       .IsRequired(false);
                e.Property(x => x.IsLimited).IsRequired().HasDefaultValue(false);

                // Tags: IReadOnlyList<string> → 백필드 _tags 를 text[]로 매핑
                e.Property<List<string>>("_tags")
                    .HasColumnName("Tags")
                    .HasColumnType("text[]")
                    .HasDefaultValueSql("'{}'::text[]")
                    .IsRequired();

                // Meta: JSON 문자열을 jsonb 로 저장
                e.Property(x => x.MetaJson)
                    .HasColumnName("Meta")
                    .HasColumnType("jsonb")
                    .IsRequired(false);

                // 인덱스
                e.HasIndex(x => x.Name);
                e.HasIndex(x => x.ElementId);
                e.HasIndex(x => x.RarityId);
                e.HasIndex(x => x.RoleId);
                e.HasIndex(x => x.FactionId);
                e.HasIndex(x => x.IsLimited);
            });
        }
        public void Modeling_CharacterStatProgression(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CharacterStatProgression>(e =>
            {
                e.ToTable("CharacterStatProgression");

                // 복합 PK
                e.HasKey(x => new { x.CharacterId, x.Level });

                // 기본 컬럼
                e.Property(x => x.CharacterId).IsRequired();
                e.Property(x => x.Level).IsRequired();

                e.Property(x => x.HP).IsRequired();
                e.Property(x => x.ATK).IsRequired();
                e.Property(x => x.DEF).IsRequired();
                e.Property(x => x.SPD).IsRequired();

                e.Property(x => x.CriRate).HasPrecision(5, 2).HasDefaultValue(5m).IsRequired();
                e.Property(x => x.CriDamage).HasPrecision(6, 2).HasDefaultValue(150m).IsRequired();

                // FK
                e.HasOne(x => x.Character)
                    .WithMany()
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);

                // 체크 제약
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("ck_csp_level", "\"Level\" >= 1");
                    t.HasCheckConstraint("ck_csp_stats", "\"HP\" >= 0 AND \"ATK\" >= 0 AND \"DEF\" >= 0 AND \"SPD\" >= 0");
                    t.HasCheckConstraint("ck_csp_cr", "\"CritRate\" >= 0 AND \"CritRate\" <= 100");
                    t.HasCheckConstraint("ck_csp_cd", "\"CritDamage\" >= 0 AND \"CritDamage\" <= 1000");
                });

                e.HasIndex(x => x.CharacterId);
            });
        }
        public void Modeling_CharacterPromotion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CharacterPromotion>(e =>
            {
                e.ToTable("CharacterPromotion");

                // 복합 PK
                e.HasKey(x => new { x.CharacterId, x.Tier });

                e.Property(x => x.MaxLevel).IsRequired();
                e.Property(x => x.CostGold).IsRequired();

                // JSONB 매핑 (값 객체/리스트)
                e.Property(x => x.Bonus)
                    .HasColumnType("jsonb")
                    .IsRequired(false);

                e.Property<List<PromotionMaterial>>("_materials")
          .HasColumnName("Materials")
          .HasColumnType("jsonb")
          .IsRequired();

                // FK
                e.HasOne(x => x.Character)
                    .WithMany()
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);

                // 체크 제약
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("ck_cp_tier", "\"Tier\" >= 0");
                    t.HasCheckConstraint("ck_cp_maxlevel", "\"MaxLevel\" >= 1");
                    t.HasCheckConstraint("ck_cp_gold", "\"CostGold\" >= 0");
                });

                e.HasIndex(x => x.CharacterId);
            });
        }
        public void Modeling_CharacterSkill(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CharacterSkill>(e =>
            {
                e.ToTable("CharacterSkills");

                // 복합 PK: (CharacterId, Slot)
                e.HasKey(x => new { x.CharacterId, x.Slot });

                e.Property(x => x.CharacterId).IsRequired();

                e.Property(x => x.Slot).HasConversion<short>().IsRequired();

                e.Property(x => x.SkillId).IsRequired();

                e.Property(x => x.UnlockTier).HasDefaultValue(0);
                e.Property(x => x.UnlockLevel).HasDefaultValue(0);

                // 고유 제약: 캐릭터 내 동일 스킬 중복 방지
                e.HasAlternateKey(x => new { x.CharacterId, x.SkillId });

                // FK
                e.HasOne(x => x.Character)
                    .WithMany()
                    .HasForeignKey(x => x.CharacterId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Skills 테이블과 FK (삭제 제한 권장)
                e.HasOne<Skill>()
                    .WithMany()
                    .HasForeignKey(x => x.SkillId)
                    .OnDelete(DeleteBehavior.Restrict);

                // 체크 제약 (필요시)
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("ck_cs_unlock_tier", "\"UnlockTier\" >= 0");
                    t.HasCheckConstraint("ck_cs_unlock_level", "\"UnlockLevel\" >= 1");
                    // 슬롯 범위가 1~4라면:
                    t.HasCheckConstraint("ck_cs_slot", "\"Slot\" BETWEEN 1 AND 4");
                });

                e.HasIndex(x => x.SkillId);
            });
        }

        private static void Modeling_Combat(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<CombatRecord>();
            e.ToTable("Combat");
            e.HasKey(x => x.Id);

            e.Property(x => x.Mode)
                .IsRequired()
                .HasColumnName("Mode");

            e.Property(x => x.StageId)
                .HasColumnName("StageId");

            e.Property(x => x.Seed)
                .IsRequired()
                .HasColumnName("Seed");

            e.Property(x => x.InputJson)
                .IsRequired()
                .HasColumnName("InputJson")
                .HasColumnType("jsonb");

            e.Property(x => x.Result)
                .HasColumnName("Result");

            e.Property(x => x.ClearMs)
                .HasColumnName("Clear_ms");

            e.Property(x => x.BalanceVersion)
                .HasColumnName("BalanceVersion");

            e.Property(x => x.ClientVersion)
                .HasColumnName("ClientVersion");

            e.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnName("CreatedAt");

            // 인덱스
            e.HasIndex(x => x.StageId);
            e.HasIndex(x => x.Mode);
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("idx_combat_created_at");
        }

        private static void Modeling_CombatLog(ModelBuilder modelBuilder)
        {
            var e = modelBuilder.Entity<CombatLogRecord>();
            e.ToTable("CombatLog");
            e.HasKey(x => x.Id);

            e.Property(x => x.CombatId)
                .IsRequired()
                .HasColumnName("CombatId");

            e.Property(x => x.TMs)
                .IsRequired()
                .HasColumnName("t_ms");

            e.Property(x => x.PayloadJson)
                .IsRequired()
                .HasColumnName("PayloadJson")
                .HasColumnType("jsonb");

            e.HasIndex(x => new { x.CombatId, x.TMs })
                .HasDatabaseName("idx_combat_log_order");

            e.HasOne(x => x.Combat)
                .WithMany(c => c.Logs)
                .HasForeignKey(x => x.CombatId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        private static string? JsonNodeToString(JsonNode? node)
    => node is null ? null : node.ToJsonString();

        private static JsonNode? StringToJsonNode(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            // 이 메서드 내부에서는 optional 인수 사용 가능
            return JsonNode.Parse(json); // 필요하면 옵션 명시 가능: JsonNode.Parse(json, null, default)
        }

        // 2) ValueConverter는 래퍼를 호출만 함 (Expression에 금지 요소 없음)
        private static readonly ValueConverter<JsonNode?, string?> JsonNodeConverter =
            new(v => JsonNodeToString(v), v => StringToJsonNode(v));

        private void Modeling_Item(ModelBuilder mb)
        {
            mb.Entity<Item>(e =>
            {
                e.ToTable("Item");
                e.HasKey(x => x.Id);

                e.Property(x => x.Code).IsRequired();
                e.HasIndex(x => x.Code).IsUnique();

                e.Property(x => x.Name).IsRequired();
                e.Property(x => x.Description).HasDefaultValue("");

                // FK 값들: (참조 테이블은 다른 모델에서 구성되어 있다고 가정)
                e.Property(x => x.TypeId).IsRequired();
                e.Property(x => x.RarityId).IsRequired();

                e.Property(x => x.Stackable).HasDefaultValue(true);
                e.Property(x => x.MaxStack).HasDefaultValue(99);
                e.Property(x => x.Tradable).HasDefaultValue(true);
                e.Property(x => x.Weight).HasDefaultValue(0);

                // string[] -> text[] (Npgsql이 자동 매핑)
                e.Property(x => x.Tags).HasColumnType("text[]").HasDefaultValue(new string[] { });

                // JsonNode -> jsonb
                e.Property(x => x.Meta).HasColumnType("jsonb");



                e.Property(x => x.CreatedAt).HasColumnType("timestamptz");
                e.Property(x => x.UpdatedAt).HasColumnType("timestamptz");


                e.HasMany(i => i.Stats)
                 .WithOne()
                 .HasForeignKey(x => x.ItemId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Navigation(i => i.Stats).HasField("_stats")
                                           .UsePropertyAccessMode(PropertyAccessMode.Field);


                e.HasMany(i => i.Effects)
  .WithOne()
  .HasForeignKey(x => x.ItemId)
  .OnDelete(DeleteBehavior.Cascade);
                e.Navigation(i => i.Effects).HasField("_effects")
                                             .UsePropertyAccessMode(PropertyAccessMode.Field);


                e.HasMany(i => i.Prices)
                 .WithOne()
                 .HasForeignKey(x => x.ItemId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Navigation(i => i.Prices).HasField("_prices")
                                .UsePropertyAccessMode(PropertyAccessMode.Field);
            });
        }
        private void Modeling_ItemStat(ModelBuilder mb)
        {
            mb.Entity<ItemStat>(e =>
            {
                e.ToTable("ItemStat");
                e.HasKey(x => x.Id);
                e.Property(x => x.Value).HasColumnType("numeric(12,4)");
                e.HasIndex(x => new { x.ItemId, x.StatId }).IsUnique();
            });
        }

        private void Modeling_ItemEffect(ModelBuilder mb)
        {
            mb.Entity<ItemEffect>(e =>
            {
                e.ToTable("ItemEffect");
                e.HasKey(x => x.Id);

                e.Property(x => x.Payload).HasColumnType("jsonb");
                e.Property(x => x.SortOrder).HasDefaultValue((short)0);

                // Scope는 enum → text 저장을 원하면 컨버터 추가 가능
                // e.Property(x => x.Scope).HasConversion<string>();
            });
        }

        private void Modeling_ItemPrice(ModelBuilder mb)
        {
            mb.Entity<ItemPrice>(e =>
            {
                e.ToTable("ItemPrice");
                e.HasKey(x => x.Id);
                e.Property(x => x.Price).HasColumnType("bigint");
                e.HasIndex(x => new { x.ItemId, x.CurrencyId, x.PriceType }).IsUnique();
            });
        }
        private static void Modeling_StatTypes(ModelBuilder mb)
        {
            var e = mb.Entity<StatType>();
            e.ToTable("StatTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id)
                .ValueGeneratedOnAdd(); // smallserial과 매칭
            e.Property(x => x.Code).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.IsPercent).IsRequired();
        }
        private static void Modeling_EquipSlot(ModelBuilder mb)
        {
            mb.Entity<EquipSlot>(e =>
            {
                e.ToTable("EquipSlots");
                e.HasKey(x => x.Id);
                e.Property(x => x.Code).IsRequired();
                e.Property(x => x.Name).IsRequired();
                e.Property(x => x.SortOrder).HasDefaultValue((short)0);
                e.HasIndex(x => x.Code).IsUnique();
            });
        }

        private static void Modeling_ItemType(ModelBuilder mb)
        {
            mb.Entity<ItemType>(e =>
            {
                e.ToTable("ItemType");
                e.HasKey(x => x.Id);
                e.Property(x => x.Code).IsRequired();
                e.Property(x => x.Name).IsRequired();
                e.HasIndex(x => x.Code).IsUnique();

                e.HasOne(x => x.Slot)
                 .WithMany()
                 .HasForeignKey(x => x.SlotId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }
        private void Modeling_Currency(ModelBuilder mb)
        {
            mb.Entity<Currency>(e =>
            {
                e.ToTable("Currencies");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.Property(x => x.Code).IsRequired();
                e.Property(x => x.Name).IsRequired();
                e.HasIndex(x => x.Code).IsUnique();
            });
        }
        private void Modeling_GatchaBanner(ModelBuilder b)
        {
            b.Entity<GachaBanner>(e =>
            {
                e.ToTable("GachaBanner");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("BannerId");
                e.HasIndex(x => x.Key).IsUnique();
                e.Property(x => x.Status).HasConversion<short>();   // smallint 매핑
                e.Property(x => x.StartsAt).HasColumnName("StartsAt");
                e.Property(x => x.EndsAt).HasColumnName("EndsAt");
                // 필요한 컬럼 매핑 추가…
            });
        }
        public void Modeling_GachaPool(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GachaPool>(e =>
            {
                e.ToTable("GachaPool");
                e.HasKey(x => x.PoolId);
                e.Property(x => x.PoolId).ValueGeneratedOnAdd();

                e.Property(x => x.Name).IsRequired();

                e.Property(x => x.ScheduleStart).IsRequired();
                e.Property(x => x.ScheduleEnd);

                // jsonb 매핑 (문자열을 그대로 jsonb 컬럼에 보관)
                e.Property(x => x.PityJson).HasColumnType("jsonb");
                e.Property(x => x.Config).HasColumnType("jsonb");
                e.Property(x => x.TablesVersion);

                // 관계: 1 → N
                e.HasMany<GachaPoolEntry>()
                 .WithOne()
                 .HasForeignKey(en => en.PoolId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GachaPoolEntry>(e =>
            {
                e.ToTable("GachaPoolEntry");

                // 복합 PK (PoolId + CharacterId)
                e.HasKey(x => new { x.PoolId, x.CharacterId });

                e.Property(x => x.PoolId).IsRequired();
                e.Property(x => x.CharacterId).IsRequired();
                e.Property(x => x.Grade).IsRequired();
                e.Property(x => x.RateUp).HasDefaultValue(false);
                e.Property(x => x.Weight).IsRequired();

                // 간단 체크 제약(포스트그레스일 때 유효)
                e.HasCheckConstraint("ck_gpe_weight_pos", "\"Weight\" > 0");
            });
        }
        private void Modeling_Synergy(ModelBuilder b)
        {
            b.Entity<Synergy>(ConfigureSynergy);
            b.Entity<SynergyBonus>(ConfigureBonus);
            b.Entity<SynergyRule>(ConfigureRule);
            //b.Entity<SynergyTarget>(ConfigureTarget);
        }

        private static void ConfigureSynergy(EntityTypeBuilder<Synergy> e)
        {
            e.ToTable("Synergy");
            e.HasKey(x => x.SynergyId);
            e.Property(x => x.SynergyId).HasColumnName("SynergyId");

            e.Property(x => x.Key).IsRequired();
            e.HasIndex(x => x.Key).IsUnique();

            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Description).IsRequired();

            e.Property(x => x.IconId).HasColumnName("IconId");

            e.Property(x => x.Effect).HasColumnName("Effect"); // jsonb ← JsonDocument (Npgsql가 자동 매핑)
            e.Property(x => x.Stacking).HasConversion<short>(); // smallint

            e.Property(x => x.IsActive).HasColumnName("IsActive");
            e.Property(x => x.StartAt).HasColumnName("StartAt");
            e.Property(x => x.EndAt).HasColumnName("EndAt");

            // 관계
            e.HasMany(x => x.Bonuses)
             .WithOne(x => x.Synergy!)
             .HasForeignKey(x => x.SynergyId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Rules)
             .WithOne(x => x.Synergy!)
             .HasForeignKey(x => x.SynergyId)
             .OnDelete(DeleteBehavior.Cascade);

            //e.HasMany(x => x.Targets)
            // .WithOne(x => x.Synergy!)
            // .HasForeignKey(x => x.SynergyId)
            // .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureBonus(EntityTypeBuilder<SynergyBonus> e)
        {
            e.ToTable("SynergyBonus");
            e.HasKey(x => new { x.SynergyId, x.Threshold }); // 복합 PK
            e.Property(x => x.SynergyId).HasColumnName("SynergyId");
            e.Property(x => x.Threshold).HasColumnName("Threshold");
            e.Property(x => x.Effect).HasColumnName("Effect"); // jsonb
            e.Property(x => x.Note).HasColumnName("Note");
        }

        private static void ConfigureRule(EntityTypeBuilder<SynergyRule> e)
        {
            e.ToTable("SynergyRule");
            e.HasKey(x => new { x.SynergyId, x.Scope, x.Metric, x.RefId }); // 복합 PK

            e.Property(x => x.SynergyId).HasColumnName("SynergyId");
            e.Property(x => x.Scope).HasConversion<short>().HasColumnName("Scope");
            e.Property(x => x.Metric).HasConversion<short>().HasColumnName("Metric");
            e.Property(x => x.RefId).HasColumnName("RefId");
            e.Property(x => x.RequiredCnt).HasColumnName("RequiredCnt");
            e.Property(x => x.Extra).HasColumnName("Extra"); // jsonb nullable

            // 성능 인덱스(후보 조회)
            e.HasIndex(x => new { x.Metric, x.RefId }).HasDatabaseName("ix_rule_metric_ref");
        }

        //private static void ConfigureTarget(EntityTypeBuilder<SynergyTarget> e)
        //{
        //    e.ToTable("SynergyTarget");
        //    e.HasKey(x => new { x.SynergyId, x.TargetType, x.TargetId }); // 복합 PK

        //    e.Property(x => x.SynergyId).HasColumnName("SynergyId");
        //    e.Property(x => x.TargetType).HasConversion<short>().HasColumnName("TargetType");
        //    e.Property(x => x.TargetId).HasColumnName("TargetId");

        //    e.HasIndex(x => new { x.TargetType, x.TargetId }).HasDatabaseName("ix_target_type_id");
        //}
    }
}