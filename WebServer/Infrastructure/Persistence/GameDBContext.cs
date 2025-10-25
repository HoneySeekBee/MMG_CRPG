using Domain.Entities;
using Domain.Entities.User;
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
        public DbSet<Portrait> Portraits => Set<Portrait>();
        public DbSet<Skill> Skills => Set<Skill>();
        public DbSet<SkillLevel> SkillLevels => Set<SkillLevel>();
        public DbSet<Character> Characters => Set<Character>();
        public DbSet<CharacterSkill> CharacterSkills => Set<CharacterSkill>();
        public DbSet<CharacterStatProgression> CharacterStatProgressions => Set<CharacterStatProgression>();
        public DbSet<CharacterPromotionMaterial> CharacterMaterials => Set<CharacterPromotionMaterial>();
        public DbSet<CharacterPromotion> CharacterPromotions => Set<CharacterPromotion>();
        public DbSet<CombatRecord> Combats => Set<CombatRecord>();
        public DbSet<CombatLogRecord> CombatLogs => Set<CombatLogRecord>();

        public DbSet<Item> Items => Set<Item>();
        public DbSet<ItemStat> ItemStats => Set<ItemStat>();
        public DbSet<ItemEffect> ItemEffects => Set<ItemEffect>();
        public DbSet<ItemPrice> ItemPrices => Set<ItemPrice>();

        public DbSet<ItemType> ItemTypes => Set<ItemType>();
        public DbSet<StatType> StatTypes => Set<StatType>();

        public DbSet<EquipSlot> EquipSlots => Set<EquipSlot>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<GachaBanner> GachaBanners => Set<GachaBanner>();

        public DbSet<GachaPool> GachaPools => Set<GachaPool>();
        public DbSet<GachaPoolEntry> GachaPoolEntries => Set<GachaPoolEntry>();

        public DbSet<Synergy> Synergies => Set<Synergy>();
        public DbSet<SynergyRule> SynergyRules => Set<SynergyRule>();
        public DbSet<SynergyBonus> SynergyBonuses => Set<SynergyBonus>();
        //public DbSet<SynergyTarget> SynergyTargets => Set<SynergyTarget>();

        public DbSet<User> Users => Set<User>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();

        public DbSet<Stage> Stages => Set<Stage>();
        public DbSet<StageWave> StageWaves => Set<StageWave>();
        public DbSet<StageWaveEnemy> StageWaveEnemies => Set<StageWaveEnemy>();
        public DbSet<StageDrop> StageDrops => Set<StageDrop>();
        public DbSet<StageFirstClearReward> StageFirstClearRewards => Set<StageFirstClearReward>();
        public DbSet<StageRequirement> StageRequirements => Set<StageRequirement>();
        public DbSet<UserStageProgress> UserStageProgresses => Set<UserStageProgress>();
        public DbSet<UserCurrency> UserCurrencies => Set<UserCurrency>();
        public DbSet<UserInventory> UserInventories => Set<UserInventory>();

        public DbSet<UserCharacter> UserCharacters => Set<UserCharacter>();
        public DbSet<UserCharacterSkill> UserCharacterSkills => Set<UserCharacterSkill>();
        public DbSet<UserCharacterEquip> UserCharacterEquips => Set<UserCharacterEquip>();
        public DbSet<CharacterExp> CharacterExps => Set<CharacterExp>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Console.WriteLine("OnModelCreateing");
 
            Modeling_Skill(modelBuilder);
            Modeling_SkillLevel(modelBuilder);

            modelBuilder.Ignore<StatModifier>();
            modelBuilder.Ignore<PromotionMaterial>();
             
            Modeling_Combat(modelBuilder);
            Modeling_CombatLog(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(GameDBContext).Assembly);

            Modeling_StatTypes(modelBuilder);
            Modeling_ItemType(modelBuilder);
            Modeling_EquipSlot(modelBuilder);
            Modeling_Currency(modelBuilder);
            Modeling_GatchaBanner(modelBuilder);
            Modeling_GachaPool(modelBuilder);

            Modeling_Synergy(modelBuilder);
            OnModelCreating_User(modelBuilder);

            OnModelCreating_Stage(modelBuilder);
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
                e.HasIndex(x => x.IconId);
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
                e.Property(x => x.Active).IsRequired();
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
            modelBuilder.Entity<GachaPool>(g =>
            {
                g.ToTable("GachaPool");
                g.HasKey(x => x.PoolId);
                g.Property(x => x.PoolId).ValueGeneratedOnAdd();
                g.Property(x => x.Name).IsRequired();

                g.Property(x => x.ScheduleStart).IsRequired().HasColumnType("timestamp with time zone");
                g.Property(x => x.ScheduleEnd).IsRequired(false).HasColumnType("timestamp with time zone");

                g.Property(x => x.PityJson).HasColumnType("jsonb");
                g.Property(x => x.Config).HasColumnType("jsonb");
                g.Property(x => x.TablesVersion);

                // 읽기 전용 컬렉션 백킹 필드
                g.Metadata.FindNavigation(nameof(GachaPool.Entries))!
                 .SetPropertyAccessMode(PropertyAccessMode.Field);

                // 네비게이션을 이용해 "하나의 관계"로 고정
                g.HasMany(x => x.Entries)
                 .WithOne()                       // GachaPoolEntry에 네비가 없으니 WithOne()
                 .HasForeignKey(x => x.PoolId)
                 .IsRequired()
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GachaPoolEntry>(e =>
            {
                e.ToTable("GachaPoolEntry");
                e.HasKey(x => new { x.PoolId, x.CharacterId });

                e.Property(x => x.PoolId).IsRequired();
                e.Property(x => x.CharacterId).IsRequired();
                e.Property(x => x.Grade).IsRequired();
                e.Property(x => x.RateUp).HasDefaultValue(false);
                e.Property(x => x.Weight).IsRequired();

                e.HasCheckConstraint("ck_gpe_weight_pos", "\"Weight\" > 0");

                // 안전장치: 혹시 섀도우 속성이 남아있으면 무시
                e.Ignore("GachaPoolPoolId");   // 섀도우 FK 강제 무시
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


        private void OnModelCreating_User(ModelBuilder b)
        { 
            Modeling_Session(b);
            Modeling_SecurityEvent(b); 
            Modeling_CharacterExp(b); 
        }
         
        private static void Modeling_CharacterExp(ModelBuilder b)
        {
            b.Entity<CharacterExp>(e =>
            {
                e.ToTable("CharacterExp");

                // PK (레어도, 레벨)
                e.HasKey(x => new { x.RarityId, x.Level });

                // 컬럼 매핑
                e.Property(x => x.RarityId).HasColumnName("RarityId");
                e.Property(x => x.Level).HasColumnName("Level");
                e.Property(x => x.RequiredExp).HasColumnName("RequiredExp").IsRequired();

                // 자주 조회하면 추가 인덱스
                e.HasIndex(x => x.Level);
            });
        }

        // ---------- Session ----------
        private void Modeling_Session(ModelBuilder b)
        {
            b.Entity<Session>(ConfigureSession);
        }

        private static void ConfigureSession(EntityTypeBuilder<Session> e)
        {
            e.ToTable("Sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();

            e.Property(x => x.UserId).IsRequired();

            e.Property(x => x.AccessTokenHash).IsRequired().HasMaxLength(64);
            e.Property(x => x.RefreshTokenHash).IsRequired().HasMaxLength(64);

            e.Property(x => x.ExpiresAt).IsRequired();
            e.Property(x => x.RefreshExpiresAt).IsRequired();
            e.Property(x => x.Revoked).IsRequired();
            e.Property(x => x.CreatedAt).IsRequired();

            e.HasIndex(x => new { x.UserId, x.Revoked, x.ExpiresAt });
            e.HasIndex(x => x.RefreshTokenHash).IsUnique();
        }

        // ---------- SecurityEvent ----------
        private void Modeling_SecurityEvent(ModelBuilder b)
        {
            b.Entity<SecurityEvent>(ConfigureSecurityEvent);
        }

        private static void ConfigureSecurityEvent(EntityTypeBuilder<SecurityEvent> e)
        {
            e.ToTable("SecurityEvents");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();

            e.Property(x => x.UserId).IsRequired(false);
            e.Property(x => x.Type).IsRequired();     // enum
            e.Property(x => x.Meta)
      .HasColumnName("Meta")
      .HasColumnType("jsonb")
      .IsRequired(false);
            // jsonb 매핑(문자열)
            e.Property(x => x.CreatedAt).IsRequired();

            e.HasIndex(x => new { x.UserId, x.CreatedAt });
        }
        private void OnModelCreating_Stage(ModelBuilder b)
        {
            Modeling_Stage(b);
            Modeling_StageWave(b);
            Modeling_StageWaveEnemy(b);
            Modeling_StageDrop(b);
            Modeling_StageFirstClearReward(b);
            Modeling_StageRequirement(b);
            Modeling_UserStageProgress(b);
            // SecurityEvents는 기존 Modeling_SecurityEvent 사용
        }

        // ---------- Stage ----------
        private static void Modeling_Stage(ModelBuilder b)
        {
            b.Entity<Stage>(ConfigureStage);
        }

        private static void ConfigureStage(EntityTypeBuilder<Stage> e)
        {
            e.ToTable("Stages");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();

            e.Property(x => x.Chapter).HasColumnName("Chapter").IsRequired();
            e.Property(x => x.Order).HasColumnName("Order").IsRequired();

            e.Property(x => x.Name)
             .HasColumnName("Name")
             .HasMaxLength(64)            // 요구 시 50으로 줄여도 OK
             .IsRequired(false);

            e.Property(x => x.RecommendedPower)
             .HasColumnName("RecommendedPower")
             .HasColumnType("smallint")
             .IsRequired();

            e.Property(x => x.StaminaCost)
             .HasColumnName("StaminaCost")
             .HasColumnType("smallint")
             .IsRequired();

            e.Property(x => x.IsActive)
             .HasColumnName("IsActive")
             .HasDefaultValue(true)
             .IsRequired();

            // 유니크: (Chapter, Order)
            e.HasIndex(x => new { x.Chapter, x.Order }).IsUnique();

            // 관계 (Cascade)
            e.HasMany(x => x.Waves)
             .WithOne()
             .HasForeignKey(w => w.StageId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Drops)
             .WithOne()
             .HasForeignKey(d => d.StageId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.FirstRewards)
             .WithOne()
             .HasForeignKey(r => r.StageId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Requirements)
             .WithOne()
             .HasForeignKey(r => r.StageId)
             .OnDelete(DeleteBehavior.Cascade);
        }

        // ---------- StageWaves ----------
        private static void Modeling_StageWave(ModelBuilder b)
        {
            b.Entity<StageWave>(ConfigureStageWave);
        }

        private static void ConfigureStageWave(EntityTypeBuilder<StageWave> e)
        {
            e.ToTable("StageWaves");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.StageId).HasColumnName("StageId").IsRequired();
            e.Property(x => x.Index).HasColumnName("Index").HasColumnType("smallint").IsRequired();

            e.HasIndex(x => new { x.StageId, x.Index }).IsUnique(); // 웨이브 순번 중복 방지

            e.HasMany(x => x.Enemies)
             .WithOne()
             .HasForeignKey(en => en.StageWaveId)
             .OnDelete(DeleteBehavior.Cascade);
        }

        // ---------- StageWaveEnemies ----------
        private static void Modeling_StageWaveEnemy(ModelBuilder b)
        {
            b.Entity<StageWaveEnemy>(ConfigureStageWaveEnemy);
        }

        private static void ConfigureStageWaveEnemy(EntityTypeBuilder<StageWaveEnemy> e)
        {
            e.ToTable("StageWaveEnemies");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.StageWaveId).HasColumnName("StageWaveId").IsRequired();
            e.Property(x => x.EnemyCharacterId).HasColumnName("EnemyCharacterId").IsRequired();
            e.Property(x => x.Level).HasColumnName("Level").HasColumnType("smallint").IsRequired();
            e.Property(x => x.Slot).HasColumnName("Slot").HasColumnType("smallint").IsRequired();
            e.Property(x => x.AiProfile).HasColumnName("AiProfile").IsRequired(false);

            e.HasIndex(x => x.StageWaveId);
            e.HasIndex(x => new { x.StageWaveId, x.Slot }).IsUnique(); // 슬롯 중복 방지

            // FK는 상단 StageWave에서 Cascade 설정 이미 수행됨
        }

        // ---------- StageDrops ----------
        private static void Modeling_StageDrop(ModelBuilder b)
        {
            b.Entity<StageDrop>(ConfigureStageDrop);
        }

        private static void ConfigureStageDrop(EntityTypeBuilder<StageDrop> e)
        {
            e.ToTable("StageDrops");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.StageId).HasColumnName("StageId").IsRequired();
            e.Property(x => x.ItemId).HasColumnName("ItemId").IsRequired();

            e.Property(x => x.Rate)
             .HasColumnName("Rate")
             .HasColumnType("numeric(6,5)")    // 또는 .HasPrecision(6,5)
             .IsRequired();

            e.Property(x => x.MinQty).HasColumnName("MinQty").HasColumnType("smallint").IsRequired();
            e.Property(x => x.MaxQty).HasColumnName("MaxQty").HasColumnType("smallint").IsRequired();

            e.Property(x => x.FirstClearOnly)
             .HasColumnName("FirstClearOnly")
             .HasDefaultValue(false)
             .IsRequired();

            e.HasIndex(x => x.StageId);
            e.HasIndex(x => x.ItemId);
            // 옵션: e.HasIndex(x => new { x.StageId, x.ItemId, x.FirstClearOnly }).IsUnique();
        }

        // ---------- StageFirstClearRewards ----------
        private static void Modeling_StageFirstClearReward(ModelBuilder b)
        {
            b.Entity<StageFirstClearReward>(ConfigureStageFirstClearReward);
        }

        private static void ConfigureStageFirstClearReward(EntityTypeBuilder<StageFirstClearReward> e)
        {
            e.ToTable("StageFirstClearRewards");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.StageId).HasColumnName("StageId").IsRequired();
            e.Property(x => x.ItemId).HasColumnName("ItemId").IsRequired();
            e.Property(x => x.Qty).HasColumnName("Qty").HasColumnType("smallint").IsRequired();

            e.HasIndex(x => x.StageId);
            e.HasIndex(x => x.ItemId);
            // 옵션: e.HasIndex(x => new { x.StageId, x.ItemId }).IsUnique();
        }

        // ---------- StageRequirements ----------
        private static void Modeling_StageRequirement(ModelBuilder b)
        {
            b.Entity<StageRequirement>(ConfigureStageRequirement);
        }

        private static void ConfigureStageRequirement(EntityTypeBuilder<StageRequirement> e)
        {
            e.ToTable("StageRequirements");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();

            e.Property(x => x.StageId).HasColumnName("StageId").IsRequired();
            e.Property(x => x.RequiredStageId).HasColumnName("RequiredStageId").IsRequired(false);
            e.Property(x => x.MinAccountLevel).HasColumnName("MinAccountLevel").HasColumnType("smallint").IsRequired(false);

            e.HasIndex(x => x.StageId);
            e.HasIndex(x => x.RequiredStageId);

            e.HasOne<Stage>()                      // self-ref: RequiredStageId
             .WithMany()
             .HasForeignKey(x => x.RequiredStageId)
             .OnDelete(DeleteBehavior.SetNull);    // 운영 중 삭제 안전

            // 부모 Stage와의 관계는 Stage에서 Cascade already
        }

        // ---------- UserStageProgresses ----------
        private static void Modeling_UserStageProgress(ModelBuilder b)
        {
            b.Entity<UserStageProgress>(ConfigureUserStageProgress);
        }

        private static void ConfigureUserStageProgress(EntityTypeBuilder<UserStageProgress> e)
        {
            e.ToTable("UserStageProgresses");
            e.HasKey(x => new { x.UserId, x.StageId });

            e.Property(x => x.UserId).HasColumnName("UserId").IsRequired();
            e.Property(x => x.StageId).HasColumnName("StageId").IsRequired();

            e.Property(x => x.Cleared)
             .HasColumnName("Cleared")
             .HasDefaultValue(false)
             .IsRequired();

            e.Property(x => x.Stars)
             .HasColumnName("Stars")
             .HasColumnType("smallint")
             .IsRequired();

            e.Property(x => x.ClearedAt)
             .HasColumnName("ClearedAt")
             .IsRequired(false);

            e.HasIndex(x => x.StageId);

            e.HasOne<Stage>()
             .WithMany()
             .HasForeignKey(x => x.StageId)
             .OnDelete(DeleteBehavior.Restrict); // 유저 데이터 보호

            e.HasOne<User>()
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}