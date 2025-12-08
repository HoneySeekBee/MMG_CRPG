using Domain.Entities;
using Domain.Entities.Characters;
using Domain.Entities.Contents;
using Domain.Entities.Gacha;
using Domain.Entities.Monsters;
using Domain.Entities.Skill;
using Domain.Entities.User;
using Infrastructure.Persistence.Configurations.Characters;
using Infrastructure.Persistence.Configurations.Contents;
using Infrastructure.Persistence.Configurations.Gacha;
using Infrastructure.Persistence.Configurations.Items;
using Infrastructure.Persistence.Configurations.MasterData;
using Infrastructure.Persistence.Configurations.Monsters;
using Infrastructure.Persistence.Configurations.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;
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

        public DbSet<CharacterModel> CharacterModels => Set<CharacterModel>();
        public DbSet<CharacterModelPart> CharacterModelParts => Set<CharacterModelPart>();
        public DbSet<CharacterModelWeapon> CharacterModelWeapons => Set<CharacterModelWeapon>();

        public DbSet<Monster> Monsters => Set<Monster>();
        public DbSet<MonsterStatProgression> monsterStatProgressions => Set<MonsterStatProgression>();
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
        public DbSet<GachaDrawLog> GachaDrawLogs => Set<GachaDrawLog>();
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
        public DbSet<StageBatch> StageBatches => Set<StageBatch>();


        public DbSet<UserStageProgress> UserStageProgresses => Set<UserStageProgress>();
        public DbSet<UserCurrency> UserCurrencies => Set<UserCurrency>();
        public DbSet<UserInventory> UserInventories => Set<UserInventory>();

        public DbSet<UserCharacter> UserCharacters => Set<UserCharacter>();
        public DbSet<UserCharacterSkill> UserCharacterSkills => Set<UserCharacterSkill>();
        public DbSet<UserCharacterEquip> UserCharacterEquips => Set<UserCharacterEquip>();
        public DbSet<CharacterExp> CharacterExps => Set<CharacterExp>();
        public DbSet<UserParty> UserParties => Set<UserParty>();
        public DbSet<UserPartySlot> UserPartySlots => Set<UserPartySlot>();
        #region Contents
        public DbSet<Battle> Battles => Set<Battle>();
        public DbSet<Chapter> Chapters => Set<Chapter>();

        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 스키마를 꼭 넣기! (public)
            modelBuilder.HasPostgresEnum<Domain.Enum.Characters.BodySize>("public", "BodySize");
            modelBuilder.HasPostgresEnum<Domain.Enum.Characters.PartType>("public", "PartType");
            modelBuilder.HasPostgresEnum<Domain.Enum.Characters.CharacterAnimationType>("public", "CharacterAnimationType");

            modelBuilder.ApplyConfiguration(new CharacterConfiguration());

            modelBuilder.ApplyConfiguration(new CharacterModelConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterModelPartConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterModelWeaponConfiguration());

            modelBuilder.ApplyConfiguration(new CharacterPromotionConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterPromotionMaterialConfiguration());

            modelBuilder.ApplyConfiguration(new CharacterSkillConfiguration());
            modelBuilder.ApplyConfiguration(new CharacterStatProgressionConfiguration());

            modelBuilder.ApplyConfiguration(new MonsterConfiguration());
            modelBuilder.ApplyConfiguration(new MonsterStatProgressionConfiguration());

            modelBuilder.ApplyConfiguration(new RarityConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new FactionConfiguration());
            modelBuilder.ApplyConfiguration(new ElementConfiguration());
            modelBuilder.ApplyConfiguration(new ElementAffinityConfiguration());
            modelBuilder.ApplyConfiguration(new PortraitConfiguration());
            modelBuilder.ApplyConfiguration(new IconConfiguration());

            modelBuilder.ApplyConfiguration(new ItemConfiguration());
            modelBuilder.ApplyConfiguration(new ItemStatConfiguration());
            modelBuilder.ApplyConfiguration(new ItemEffectConfiguration());
            modelBuilder.ApplyConfiguration(new ItemPriceConfiguration());

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserCurrencyConfiguration());
            modelBuilder.ApplyConfiguration(new UserInventoryConfiguration());
            modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
            modelBuilder.ApplyConfiguration(new UserCharacterConfiguration());
            modelBuilder.ApplyConfiguration(new UserCharacterEquipConfiguration());
            modelBuilder.ApplyConfiguration(new UserPartyConfiguration());
            modelBuilder.ApplyConfiguration(new UserPartySlotConfiguration());
            modelBuilder.ApplyConfiguration(new UserCharacterSkillConfiguration());
            modelBuilder.ApplyConfiguration(new UserStageProgressConfiguration());

            #region Contents
            modelBuilder.ApplyConfiguration(new BattlesConfiguration());
            modelBuilder.ApplyConfiguration(new ChapterConfiguration());

            modelBuilder.ApplyConfiguration(new StageConfiguration());
            modelBuilder.ApplyConfiguration(new StageDropConfiguration());
            modelBuilder.ApplyConfiguration(new StageFirstClearRewardConfiguration());
            modelBuilder.ApplyConfiguration(new StageRequirementsConfiguration());
            modelBuilder.ApplyConfiguration(new StageWaveConfiguration());
            modelBuilder.ApplyConfiguration(new StageWaveEnemiesConfiguration());
            modelBuilder.ApplyConfiguration(new StageBatchConfiguration());

            #endregion

            #region Gacha
            modelBuilder.ApplyConfiguration(new GachaDrawLogConfiguration());
            modelBuilder.ApplyConfiguration(new GachaBannerConfiguration());
            modelBuilder.ApplyConfiguration(new GachaPoolConfiguration());
            modelBuilder.ApplyConfiguration(new GachaPoolEntryConfiguration());
            #endregion

            var et = modelBuilder.Model.FindEntityType(typeof(CharacterModel))!;
            var prop = et.FindProperty(nameof(CharacterModel.BodyType))!;
            Console.WriteLine($"BodyType column type = {prop.GetColumnType()}");
            Console.WriteLine("OnModelCreateing");


            Modeling_Skill(modelBuilder);
            Modeling_SkillLevel(modelBuilder);

            modelBuilder.Ignore<StatModifier>();
            modelBuilder.Ignore<PromotionMaterial>();
             
            Modeling_Combat(modelBuilder);
            Modeling_CombatLog(modelBuilder);
             
            Modeling_StatTypes(modelBuilder);
            Modeling_ItemType(modelBuilder);
            Modeling_EquipSlot(modelBuilder);
            Modeling_Currency(modelBuilder);  

            Modeling_Synergy(modelBuilder);
            OnModelCreating_User(modelBuilder);
             

             
            var p = et.FindProperty(nameof(Domain.Entities.Characters.CharacterModel.BodyType))!;
            Console.WriteLine($"ClrType={p.ClrType}, ProviderClrType={p.GetProviderClrType()}, ColumnType={p.GetColumnType()}, Converter={(p.GetValueConverter() is null ? "null" : p.GetValueConverter()!.GetType().Name)}");
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
           .HasColumnType("jsonb")
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
                .ValueGeneratedOnAdd()
                .HasColumnType("integer");
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

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            LogDataSourceHash("[SaveChanges]");
            return await base.SaveChangesAsync(ct);
        }
        private void LogDataSourceHash(string tag)
        {
            var conn = (NpgsqlConnection)Database.GetDbConnection();
            Console.WriteLine($"[Ctx {tag}] DS Hash = {conn.DataSource?.GetHashCode()}");
        }

    }
}