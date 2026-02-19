using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMySQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Battles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    scene_key = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    check_multi = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Battles", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    chapter_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    battle_id = table.Column<int>(type: "int", nullable: false),
                    chapter_num = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.chapter_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CharacterExp",
                columns: table => new
                {
                    RarityId = table.Column<short>(type: "smallint", nullable: false),
                    Level = table.Column<short>(type: "smallint", nullable: false),
                    RequiredExp = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterExp", x => new { x.RarityId, x.Level });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CharacterModelParts",
                columns: table => new
                {
                    part_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    part_key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    part_type = table.Column<string>(type: "PartType(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterModelParts", x => x.part_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CharacterModelWeapon",
                columns: table => new
                {
                    weapon_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_two_handed = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterModelWeapon", x => x.weapon_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    CharacterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RarityId = table.Column<int>(type: "int", nullable: false),
                    FactionId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ElementId = table.Column<int>(type: "int", nullable: false),
                    IconId = table.Column<int>(type: "int", nullable: true),
                    PortraitId = table.Column<int>(type: "int", nullable: true),
                    ReleaseDate = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    IsLimited = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    formation_position = table.Column<short>(type: "smallint", nullable: false),
                    Meta = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tags = table.Column<string>(type: "json", nullable: false, defaultValueSql: "(JSON_ARRAY())")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.CharacterId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Combat",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Mode = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StageId = table.Column<long>(type: "bigint", nullable: true),
                    Seed = table.Column<long>(type: "bigint", nullable: false),
                    InputJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Result = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Clear_ms = table.Column<int>(type: "int", nullable: true),
                    BalanceVersion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClientVersion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combat", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Element",
                columns: table => new
                {
                    ElementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconId = table.Column<int>(type: "int", nullable: true),
                    ColorHex = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Meta = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Element", x => x.ElementId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ElementAffinity",
                columns: table => new
                {
                    AttackerElementId = table.Column<int>(type: "int", nullable: false),
                    DefenderElementId = table.Column<int>(type: "int", nullable: false),
                    Multiplier = table.Column<decimal>(type: "numeric(4,2)", nullable: false, defaultValue: 1.00m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElementAffinity", x => new { x.AttackerElementId, x.DefenderElementId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EquipSlots",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    IconId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipSlots", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Faction",
                columns: table => new
                {
                    FactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconId = table.Column<int>(type: "int", nullable: true),
                    ColorHex = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    Meta = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faction", x => x.FactionId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GachaDrawLog",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    banner_id = table.Column<int>(type: "int", nullable: false),
                    pool_id = table.Column<int>(type: "int", nullable: false),
                    results_json = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    time_stamp = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GachaDrawLog", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GachaPool",
                columns: table => new
                {
                    PoolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScheduleStart = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ScheduleEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    PityJson = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TablesVersion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Config = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GachaPool", x => x.PoolId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Icons",
                columns: table => new
                {
                    IconId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Atlas = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    X = table.Column<int>(type: "int", nullable: true),
                    Y = table.Column<int>(type: "int", nullable: true),
                    W = table.Column<int>(type: "int", nullable: true),
                    H = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Icons", x => x.IconId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    RarityId = table.Column<int>(type: "int", nullable: false),
                    IconId = table.Column<int>(type: "int", nullable: true),
                    PortraitId = table.Column<int>(type: "int", nullable: true),
                    Stackable = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    MaxStack = table.Column<int>(type: "int", nullable: false, defaultValue: 99),
                    BindType = table.Column<int>(type: "int", nullable: false),
                    Tradable = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    DurabilityMax = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Tags = table.Column<string>(type: "json", nullable: false, defaultValueSql: "(JSON_ARRAY())")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Meta = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    EquipType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Monsters",
                columns: table => new
                {
                    monster_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    model_key = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    element_id = table.Column<int>(type: "int", nullable: true),
                    portrait_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monsters", x => x.monster_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Portraits",
                columns: table => new
                {
                    PortraitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Atlas = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    X = table.Column<int>(type: "int", nullable: true),
                    Y = table.Column<int>(type: "int", nullable: true),
                    W = table.Column<int>(type: "int", nullable: true),
                    H = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portraits", x => x.PortraitId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rarity",
                columns: table => new
                {
                    RarityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Stars = table.Column<short>(type: "smallint", nullable: false),
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ColorHex = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    Meta = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rarity", x => x.RarityId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconId = table.Column<int>(type: "int", nullable: true),
                    ColorHex = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    Meta = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SecurityEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    Meta = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityEvents", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AccessTokenHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RefreshTokenHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    RefreshExpiresAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Revoked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShopType = table.Column<short>(type: "smallint", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    SkillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconId = table.Column<int>(type: "int", nullable: false),
                    ElementId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    TargetingType = table.Column<short>(type: "smallint", nullable: false),
                    AoeShape = table.Column<short>(type: "smallint", nullable: false),
                    TargetSide = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BaseInfo = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tag = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.SkillId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Stages",
                columns: table => new
                {
                    stage_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    chapter_id = table.Column<int>(type: "int", nullable: false),
                    stage_num = table.Column<int>(type: "int", nullable: false),
                    recommended_power = table.Column<short>(type: "smallint", nullable: false),
                    stamina_cost = table.Column<short>(type: "smallint", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stages", x => x.stage_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StatTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPercent = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Synergy",
                columns: table => new
                {
                    SynergyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconId = table.Column<int>(type: "int", nullable: true),
                    Effect = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stacking = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EndAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Synergy", x => x.SynergyId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserCharacters",
                columns: table => new
                {
                    user_character_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Exp = table.Column<int>(type: "int", nullable: false),
                    BreakThrough = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCharacters", x => x.user_character_id);
                    table.UniqueConstraint("AK_UserCharacters_UserId_CharacterId", x => new { x.UserId, x.CharacterId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserInventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInventory", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserParty",
                columns: table => new
                {
                    party_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    battle_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserParty", x => x.party_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Account = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CharacterModel",
                columns: table => new
                {
                    character_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    body_type = table.Column<string>(type: "BodySize(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    animation_type = table.Column<string>(type: "CharacterAnimationType(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    weapon_l_id = table.Column<int>(type: "int", nullable: true),
                    weapon_r_id = table.Column<int>(type: "int", nullable: true),
                    part_head_id = table.Column<int>(type: "int", nullable: true),
                    part_hair_id = table.Column<int>(type: "int", nullable: true),
                    part_mouth_id = table.Column<int>(type: "int", nullable: true),
                    part_eye_id = table.Column<int>(type: "int", nullable: true),
                    part_acc_id = table.Column<int>(type: "int", nullable: true),
                    hair_color_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    skin_color_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterModel", x => x.character_id);
                    table.ForeignKey(
                        name: "FK_CharacterModel_CharacterModelParts_part_acc_id",
                        column: x => x.part_acc_id,
                        principalTable: "CharacterModelParts",
                        principalColumn: "part_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CharacterModel_CharacterModelParts_part_eye_id",
                        column: x => x.part_eye_id,
                        principalTable: "CharacterModelParts",
                        principalColumn: "part_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CharacterModel_CharacterModelParts_part_hair_id",
                        column: x => x.part_hair_id,
                        principalTable: "CharacterModelParts",
                        principalColumn: "part_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CharacterModel_CharacterModelParts_part_head_id",
                        column: x => x.part_head_id,
                        principalTable: "CharacterModelParts",
                        principalColumn: "part_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CharacterModel_CharacterModelParts_part_mouth_id",
                        column: x => x.part_mouth_id,
                        principalTable: "CharacterModelParts",
                        principalColumn: "part_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CharacterModel_CharacterModelWeapon_weapon_l_id",
                        column: x => x.weapon_l_id,
                        principalTable: "CharacterModelWeapon",
                        principalColumn: "weapon_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CharacterModel_CharacterModelWeapon_weapon_r_id",
                        column: x => x.weapon_r_id,
                        principalTable: "CharacterModelWeapon",
                        principalColumn: "weapon_id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CharacterPromotion",
                columns: table => new
                {
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    MaxLevel = table.Column<short>(type: "smallint", nullable: false),
                    Bonus = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CostGold = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterPromotion", x => new { x.CharacterId, x.Tier });
                    table.CheckConstraint("ck_cp_gold", "`CostGold` >= 0");
                    table.CheckConstraint("ck_cp_maxlevel", "`MaxLevel` >= 1");
                    table.CheckConstraint("ck_cp_tier", "`Tier` >= 0");
                    table.ForeignKey(
                        name: "FK_CharacterPromotion_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "CharacterId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CharacterStatProgression",
                columns: table => new
                {
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<short>(type: "smallint", nullable: false),
                    HP = table.Column<int>(type: "int", nullable: false),
                    ATK = table.Column<int>(type: "int", nullable: false),
                    DEF = table.Column<int>(type: "int", nullable: false),
                    SPD = table.Column<int>(type: "int", nullable: false),
                    CriRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 5m),
                    CriDamage = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false, defaultValue: 150m),
                    Range = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterStatProgression", x => new { x.CharacterId, x.Level });
                    table.CheckConstraint("ck_csp_cd", "`CriDamage` >= 0 AND `CriDamage` <= 1000");
                    table.CheckConstraint("ck_csp_cr", "`CriRate` >= 0 AND `CriRate` <= 100");
                    table.CheckConstraint("ck_csp_level", "`Level` >= 1");
                    table.CheckConstraint("ck_csp_stats", "`HP` >= 0 AND `ATK` >= 0 AND `DEF` >= 0 AND `SPD` >= 0");
                    table.ForeignKey(
                        name: "FK_CharacterStatProgression_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "CharacterId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CombatLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CombatId = table.Column<long>(type: "bigint", nullable: false),
                    t_ms = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombatLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CombatLog_Combat_CombatId",
                        column: x => x.CombatId,
                        principalTable: "Combat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ItemType",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SlotId = table.Column<short>(type: "smallint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemType_EquipSlots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "EquipSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GachaPoolEntry",
                columns: table => new
                {
                    PoolId = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<short>(type: "smallint", nullable: false),
                    RateUp = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Weight = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GachaPoolEntry", x => new { x.PoolId, x.CharacterId });
                    table.CheckConstraint("ck_gpe_weight_pos", "`Weight` > 0");
                    table.ForeignKey(
                        name: "FK_GachaPoolEntry_GachaPool_PoolId",
                        column: x => x.PoolId,
                        principalTable: "GachaPool",
                        principalColumn: "PoolId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GachaBanner",
                columns: table => new
                {
                    BannerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PortraitId = table.Column<int>(type: "int", nullable: true),
                    GachaPoolId = table.Column<int>(type: "int", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    Priority = table.Column<short>(type: "smallint", nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    cost_currency_id = table.Column<int>(type: "int", nullable: false),
                    cost = table.Column<int>(type: "int", nullable: false),
                    ticket_item_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GachaBanner", x => x.BannerId);
                    table.ForeignKey(
                        name: "FK_GachaBanner_Currencies_cost_currency_id",
                        column: x => x.cost_currency_id,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GachaBanner_Item_ticket_item_id",
                        column: x => x.ticket_item_id,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ItemEffect",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    Payload = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemEffect", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemEffect_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ItemPrice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    PriceType = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPrice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemPrice_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MonsterStatProgression",
                columns: table => new
                {
                    monster_id = table.Column<int>(type: "int", nullable: false),
                    level = table.Column<int>(type: "int", nullable: false),
                    hp = table.Column<int>(type: "int", nullable: false),
                    atk = table.Column<int>(type: "int", nullable: false),
                    def = table.Column<int>(type: "int", nullable: false),
                    spd = table.Column<int>(type: "int", nullable: false),
                    cri_rate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    cri_damage = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    range = table.Column<float>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonsterStatProgression", x => new { x.monster_id, x.level });
                    table.ForeignKey(
                        name: "FK_MonsterStatProgression_Monsters_monster_id",
                        column: x => x.monster_id,
                        principalTable: "Monsters",
                        principalColumn: "monster_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ShopProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShopId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    QuantityPerPurchase = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DailyLimit = table.Column<int>(type: "int", nullable: true),
                    WeeklyLimit = table.Column<int>(type: "int", nullable: true),
                    TotalLimit = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopProducts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopProducts_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopProducts_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CharacterSkills",
                columns: table => new
                {
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    Slot = table.Column<short>(type: "smallint", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    UnlockTier = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    UnlockLevel = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSkills", x => new { x.CharacterId, x.Slot });
                    table.CheckConstraint("ck_cs_slot", "`Slot` BETWEEN 1 AND 4");
                    table.CheckConstraint("ck_cs_unlock_level", "`UnlockLevel` >= 1");
                    table.CheckConstraint("ck_cs_unlock_tier", "`UnlockTier` >= 0");
                    table.ForeignKey(
                        name: "FK_CharacterSkills_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "CharacterId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SkillLevels",
                columns: table => new
                {
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Values = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Materials = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CostGold = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillLevels", x => new { x.SkillId, x.Level });
                    table.ForeignKey(
                        name: "FK_SkillLevels_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StageBatches",
                columns: table => new
                {
                    stage_batch_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    stage_id = table.Column<int>(type: "int", nullable: false),
                    batch_num = table.Column<int>(type: "int", nullable: false),
                    unit_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    env_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageBatches", x => x.stage_batch_id);
                    table.ForeignKey(
                        name: "FK_StageBatches_Stages_stage_id",
                        column: x => x.stage_id,
                        principalTable: "Stages",
                        principalColumn: "stage_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StageDrops",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    stage_id = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    rate = table.Column<decimal>(type: "numeric(6,5)", nullable: false),
                    min_qty = table.Column<short>(type: "smallint", nullable: false),
                    max_qty = table.Column<short>(type: "smallint", nullable: false),
                    first_clear_only = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageDrops", x => x.id);
                    table.ForeignKey(
                        name: "FK_StageDrops_Stages_stage_id",
                        column: x => x.stage_id,
                        principalTable: "Stages",
                        principalColumn: "stage_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StageFirstClearRewards",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    stage_id = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    qty = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageFirstClearRewards", x => x.id);
                    table.ForeignKey(
                        name: "FK_StageFirstClearRewards_Stages_stage_id",
                        column: x => x.stage_id,
                        principalTable: "Stages",
                        principalColumn: "stage_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StageRequirements",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    stage_id = table.Column<int>(type: "int", nullable: false),
                    required_stage_id = table.Column<int>(type: "int", nullable: true),
                    min_account_level = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageRequirements", x => x.id);
                    table.ForeignKey(
                        name: "FK_StageRequirements_Stages_required_stage_id",
                        column: x => x.required_stage_id,
                        principalTable: "Stages",
                        principalColumn: "stage_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StageRequirements_Stages_stage_id",
                        column: x => x.stage_id,
                        principalTable: "Stages",
                        principalColumn: "stage_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StageWaves",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    stage_id = table.Column<int>(type: "int", nullable: false),
                    index = table.Column<short>(type: "smallint", nullable: false),
                    batch_num = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageWaves", x => x.id);
                    table.ForeignKey(
                        name: "FK_StageWaves_Stages_stage_id",
                        column: x => x.stage_id,
                        principalTable: "Stages",
                        principalColumn: "stage_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ItemStat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    StatId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(12,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemStat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemStat_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemStat_StatTypes_StatId",
                        column: x => x.StatId,
                        principalTable: "StatTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SynergyBonus",
                columns: table => new
                {
                    SynergyId = table.Column<int>(type: "int", nullable: false),
                    Threshold = table.Column<short>(type: "smallint", nullable: false),
                    Effect = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynergyBonus", x => new { x.SynergyId, x.Threshold });
                    table.ForeignKey(
                        name: "FK_SynergyBonus_Synergy_SynergyId",
                        column: x => x.SynergyId,
                        principalTable: "Synergy",
                        principalColumn: "SynergyId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SynergyRule",
                columns: table => new
                {
                    SynergyId = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<short>(type: "smallint", nullable: false),
                    Metric = table.Column<short>(type: "smallint", nullable: false),
                    RefId = table.Column<int>(type: "int", nullable: false),
                    RequiredCnt = table.Column<int>(type: "int", nullable: false),
                    Extra = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SynergyRule", x => new { x.SynergyId, x.Scope, x.Metric, x.RefId });
                    table.ForeignKey(
                        name: "FK_SynergyRule_Synergy_SynergyId",
                        column: x => x.SynergyId,
                        principalTable: "Synergy",
                        principalColumn: "SynergyId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserCharacterEquip",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    EquipId = table.Column<int>(type: "int", nullable: false),
                    InventoryId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCharacterEquip", x => new { x.UserId, x.CharacterId, x.EquipId });
                    table.ForeignKey(
                        name: "FK_UserCharacterEquip_UserCharacters_UserId_CharacterId",
                        columns: x => new { x.UserId, x.CharacterId },
                        principalTable: "UserCharacters",
                        principalColumns: new[] { "UserId", "CharacterId" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserCharacterSkill",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCharacterSkill", x => new { x.UserId, x.CharacterId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_UserCharacterSkill_UserCharacters_UserId_CharacterId",
                        columns: x => new { x.UserId, x.CharacterId },
                        principalTable: "UserCharacters",
                        principalColumns: new[] { "UserId", "CharacterId" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserPartyCharacter",
                columns: table => new
                {
                    party_id = table.Column<long>(type: "bigint", nullable: false),
                    slot_id = table.Column<int>(type: "int", nullable: false),
                    user_character_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPartyCharacter", x => new { x.party_id, x.slot_id });
                    table.ForeignKey(
                        name: "FK_UserPartyCharacter_UserParty_party_id",
                        column: x => x.party_id,
                        principalTable: "UserParty",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserCurrency",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCurrency", x => new { x.UserId, x.CurrencyId });
                    table.ForeignKey(
                        name: "FK_UserCurrency_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCurrency_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UsersProfiles",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NickName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<short>(type: "smallint", nullable: false),
                    Exp = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<int>(type: "int", nullable: false),
                    Gem = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<int>(type: "int", nullable: false),
                    IconId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersProfiles", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_UsersProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserStageProgresses",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    Cleared = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Stars = table.Column<short>(type: "smallint", nullable: false),
                    ClearedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStageProgresses", x => new { x.UserId, x.StageId });
                    table.ForeignKey(
                        name: "FK_UserStageProgresses_Stages_StageId",
                        column: x => x.StageId,
                        principalTable: "Stages",
                        principalColumn: "stage_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserStageProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CharacterPromotionMaterials",
                columns: table => new
                {
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterPromotionMaterials", x => new { x.CharacterId, x.Tier, x.ItemId });
                    table.ForeignKey(
                        name: "FK_CharacterPromotionMaterials_CharacterPromotion_CharacterId_T~",
                        columns: x => new { x.CharacterId, x.Tier },
                        principalTable: "CharacterPromotion",
                        principalColumns: new[] { "CharacterId", "Tier" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterPromotionMaterials_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserPurchaseLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ShopProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PricePaid = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PurchasedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPurchaseLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPurchaseLogs_ShopProducts_ShopProductId",
                        column: x => x.ShopProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPurchaseLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StageWaveEnemies",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    stage_wave_id = table.Column<int>(type: "int", nullable: false),
                    enemy_character_id = table.Column<int>(type: "int", nullable: false),
                    level = table.Column<short>(type: "smallint", nullable: false),
                    slot = table.Column<short>(type: "smallint", nullable: false),
                    ai_profile = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageWaveEnemies", x => x.id);
                    table.ForeignKey(
                        name: "FK_StageWaveEnemies_StageWaves_stage_wave_id",
                        column: x => x.stage_wave_id,
                        principalTable: "StageWaves",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterExp_Level",
                table: "CharacterExp",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModel_part_acc_id",
                table: "CharacterModel",
                column: "part_acc_id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModel_part_eye_id",
                table: "CharacterModel",
                column: "part_eye_id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModel_part_hair_id",
                table: "CharacterModel",
                column: "part_hair_id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModel_part_head_id",
                table: "CharacterModel",
                column: "part_head_id");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterModel_part_mouth_id",
                table: "CharacterModel",
                column: "part_mouth_id");

            migrationBuilder.CreateIndex(
                name: "ix_cm_anim_type",
                table: "CharacterModel",
                column: "animation_type");

            migrationBuilder.CreateIndex(
                name: "ix_cm_body_type",
                table: "CharacterModel",
                column: "body_type");

            migrationBuilder.CreateIndex(
                name: "ix_cm_weapon_l",
                table: "CharacterModel",
                column: "weapon_l_id");

            migrationBuilder.CreateIndex(
                name: "ix_cm_weapon_r",
                table: "CharacterModel",
                column: "weapon_r_id");

            migrationBuilder.CreateIndex(
                name: "ix_parts_type_size",
                table: "CharacterModelParts",
                column: "part_type");

            migrationBuilder.CreateIndex(
                name: "ux_part_key",
                table: "CharacterModelParts",
                column: "part_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_weapon_code",
                table: "CharacterModelWeapon",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterPromotion_CharacterId",
                table: "CharacterPromotion",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterPromotionMaterials_ItemId",
                table: "CharacterPromotionMaterials",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_ElementId",
                table: "Characters",
                column: "ElementId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_FactionId",
                table: "Characters",
                column: "FactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_IsLimited",
                table: "Characters",
                column: "IsLimited");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_Name",
                table: "Characters",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_RarityId",
                table: "Characters",
                column: "RarityId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_RoleId",
                table: "Characters",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSkills_SkillId",
                table: "CharacterSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterStatProgression_CharacterId",
                table: "CharacterStatProgression",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "idx_combat_created_at",
                table: "Combat",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Combat_Mode",
                table: "Combat",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_Combat_StageId",
                table: "Combat",
                column: "StageId");

            migrationBuilder.CreateIndex(
                name: "idx_combat_log_order",
                table: "CombatLog",
                columns: new[] { "CombatId", "t_ms" });

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Element_IsActive_SortOrder",
                table: "Element",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Element_Key",
                table: "Element",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipSlots_Code",
                table: "EquipSlots",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipSlots_IconId",
                table: "EquipSlots",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Faction_Key",
                table: "Faction",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GachaBanner_cost_currency_id",
                table: "GachaBanner",
                column: "cost_currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_GachaBanner_Key",
                table: "GachaBanner",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GachaBanner_ticket_item_id",
                table: "GachaBanner",
                column: "ticket_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_Icons_Key",
                table: "Icons",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_Code",
                table: "Item",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemEffect_ItemId",
                table: "ItemEffect",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPrice_ItemId_CurrencyId_PriceType",
                table: "ItemPrice",
                columns: new[] { "ItemId", "CurrencyId", "PriceType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemStat_ItemId_StatId",
                table: "ItemStat",
                columns: new[] { "ItemId", "StatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemStat_StatId",
                table: "ItemStat",
                column: "StatId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemType_Code",
                table: "ItemType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemType_SlotId",
                table: "ItemType",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Portraits_Key",
                table: "Portraits",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rarity_Key",
                table: "Rarity",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_Key",
                table: "Role",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_UserId_CreatedAt",
                table: "SecurityEvents",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RefreshTokenHash",
                table: "Sessions",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId_Revoked_ExpiresAt",
                table: "Sessions",
                columns: new[] { "UserId", "Revoked", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_CurrencyId",
                table: "ShopProducts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_IsActive",
                table: "ShopProducts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_ItemId",
                table: "ShopProducts",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_ShopId",
                table: "ShopProducts",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_Code",
                table: "Shops",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_IsActive",
                table: "Shops",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_ShopType",
                table: "Shops",
                column: "ShopType");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_AoeShape",
                table: "Skills",
                column: "AoeShape");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_ElementId",
                table: "Skills",
                column: "ElementId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_IsActive",
                table: "Skills",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name",
                table: "Skills",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_TargetingType",
                table: "Skills",
                column: "TargetingType");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_TargetSide",
                table: "Skills",
                column: "TargetSide");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Type",
                table: "Skills",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_StageBatches_stage_id_batch_num",
                table: "StageBatches",
                columns: new[] { "stage_id", "batch_num" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageDrops_item_id",
                table: "StageDrops",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_StageDrops_stage_id",
                table: "StageDrops",
                column: "stage_id");

            migrationBuilder.CreateIndex(
                name: "IX_StageFirstClearRewards_item_id",
                table: "StageFirstClearRewards",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_StageFirstClearRewards_stage_id",
                table: "StageFirstClearRewards",
                column: "stage_id");

            migrationBuilder.CreateIndex(
                name: "IX_StageRequirements_required_stage_id",
                table: "StageRequirements",
                column: "required_stage_id");

            migrationBuilder.CreateIndex(
                name: "IX_StageRequirements_stage_id",
                table: "StageRequirements",
                column: "stage_id");

            migrationBuilder.CreateIndex(
                name: "IX_Stages_chapter_id_stage_num",
                table: "Stages",
                columns: new[] { "chapter_id", "stage_num" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageWaveEnemies_stage_wave_id",
                table: "StageWaveEnemies",
                column: "stage_wave_id");

            migrationBuilder.CreateIndex(
                name: "IX_StageWaveEnemies_stage_wave_id_slot",
                table: "StageWaveEnemies",
                columns: new[] { "stage_wave_id", "slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageWaves_stage_id_index",
                table: "StageWaves",
                columns: new[] { "stage_id", "index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatTypes_Code",
                table: "StatTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Synergy_Key",
                table: "Synergy",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rule_metric_ref",
                table: "SynergyRule",
                columns: new[] { "Metric", "RefId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacters_CharacterId",
                table: "UserCharacters",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacters_UpdatedAt",
                table: "UserCharacters",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacters_UserId",
                table: "UserCharacters",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacterSkill_CharacterId",
                table: "UserCharacterSkill",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacterSkill_SkillId",
                table: "UserCharacterSkill",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacterSkill_UpdatedAt",
                table: "UserCharacterSkill",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserCharacterSkill_UserId",
                table: "UserCharacterSkill",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrency_CurrencyId",
                table: "UserCurrency",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventory_ItemId",
                table: "UserInventory",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventory_UpdatedAt",
                table: "UserInventory",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserInventory_UserId",
                table: "UserInventory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_party_user_battle",
                table: "UserParty",
                columns: new[] { "user_id", "battle_id" });

            migrationBuilder.CreateIndex(
                name: "ux_upc_unique_char",
                table: "UserPartyCharacter",
                columns: new[] { "party_id", "user_character_id" },
                unique: true,
                filter: "\"user_character_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_log_user_product_date",
                table: "UserPurchaseLogs",
                columns: new[] { "UserId", "ShopProductId", "PurchasedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPurchaseLogs_PurchasedAt",
                table: "UserPurchaseLogs",
                column: "PurchasedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserPurchaseLogs_ShopProductId",
                table: "UserPurchaseLogs",
                column: "ShopProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Account",
                table: "Users",
                column: "Account",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersProfiles_UserId",
                table: "UsersProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStageProgresses_StageId",
                table: "UserStageProgresses",
                column: "StageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Battles");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "CharacterExp");

            migrationBuilder.DropTable(
                name: "CharacterModel");

            migrationBuilder.DropTable(
                name: "CharacterPromotionMaterials");

            migrationBuilder.DropTable(
                name: "CharacterSkills");

            migrationBuilder.DropTable(
                name: "CharacterStatProgression");

            migrationBuilder.DropTable(
                name: "CombatLog");

            migrationBuilder.DropTable(
                name: "Element");

            migrationBuilder.DropTable(
                name: "ElementAffinity");

            migrationBuilder.DropTable(
                name: "Faction");

            migrationBuilder.DropTable(
                name: "GachaBanner");

            migrationBuilder.DropTable(
                name: "GachaDrawLog");

            migrationBuilder.DropTable(
                name: "GachaPoolEntry");

            migrationBuilder.DropTable(
                name: "Icons");

            migrationBuilder.DropTable(
                name: "ItemEffect");

            migrationBuilder.DropTable(
                name: "ItemPrice");

            migrationBuilder.DropTable(
                name: "ItemStat");

            migrationBuilder.DropTable(
                name: "ItemType");

            migrationBuilder.DropTable(
                name: "MonsterStatProgression");

            migrationBuilder.DropTable(
                name: "Portraits");

            migrationBuilder.DropTable(
                name: "Rarity");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "SecurityEvents");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "SkillLevels");

            migrationBuilder.DropTable(
                name: "StageBatches");

            migrationBuilder.DropTable(
                name: "StageDrops");

            migrationBuilder.DropTable(
                name: "StageFirstClearRewards");

            migrationBuilder.DropTable(
                name: "StageRequirements");

            migrationBuilder.DropTable(
                name: "StageWaveEnemies");

            migrationBuilder.DropTable(
                name: "SynergyBonus");

            migrationBuilder.DropTable(
                name: "SynergyRule");

            migrationBuilder.DropTable(
                name: "UserCharacterEquip");

            migrationBuilder.DropTable(
                name: "UserCharacterSkill");

            migrationBuilder.DropTable(
                name: "UserCurrency");

            migrationBuilder.DropTable(
                name: "UserInventory");

            migrationBuilder.DropTable(
                name: "UserPartyCharacter");

            migrationBuilder.DropTable(
                name: "UserPurchaseLogs");

            migrationBuilder.DropTable(
                name: "UsersProfiles");

            migrationBuilder.DropTable(
                name: "UserStageProgresses");

            migrationBuilder.DropTable(
                name: "CharacterModelParts");

            migrationBuilder.DropTable(
                name: "CharacterModelWeapon");

            migrationBuilder.DropTable(
                name: "CharacterPromotion");

            migrationBuilder.DropTable(
                name: "Combat");

            migrationBuilder.DropTable(
                name: "GachaPool");

            migrationBuilder.DropTable(
                name: "StatTypes");

            migrationBuilder.DropTable(
                name: "EquipSlots");

            migrationBuilder.DropTable(
                name: "Monsters");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "StageWaves");

            migrationBuilder.DropTable(
                name: "Synergy");

            migrationBuilder.DropTable(
                name: "UserCharacters");

            migrationBuilder.DropTable(
                name: "UserParty");

            migrationBuilder.DropTable(
                name: "ShopProducts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Stages");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "Shops");
        }
    }
}
