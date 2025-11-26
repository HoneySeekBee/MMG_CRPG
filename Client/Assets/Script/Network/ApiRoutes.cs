using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ApiRoutes
{
    public const string AuthRegister = "/api/pb/auth/register";
    public const string Status = "/api/pb/status";
    public const string AuthLogin = "/api/pb/auth/login";
    public const string AuthGuest = "/api/pb/auth/guest"; // 예시: 게스트 로그인
    public const string AuthRefresh = "/api/pb/auth/refresh";
    public const string PlayerBootstrap = "/api/pb/player/bootstrap";

    public const string MeSummary = "/api/pb/me/summary";
    public const string MeProfile = "/api/pb/me/profile";
    public const string UserStageProgress = "/api/pb/me/stages";
    public const string MeProfileUpdate = "/api/pb/me/profile";
    public const string MeChangePassword = "/api/pb/me/change-password"; 

    public const string ItemTypes = "/api/pb/itemtypes";
    public const string EquipSlots = "/api/pb/equipslots";
    public const string MasterData = "/api/pb/masterdata";
    public const string Portraits = "/api/pb/portraits";
    public const string Icons = "/api/pb/icons";
    public const string Items = "/api/pb/items";

    public const string Skills = "/api/pb/skills";
    public const string CharacterExp = "/api/pb/character-exp";
    public const string Character = "/api/pb/characters";
    public const string CharacterModel_List = "/api/pb/character-model/list";
    public const string CharacterModel_Parts = "/api/pb/character-model/parts";
    public const string CharacterModel_Weapons = "/api/pb/character-model/weapons";


    public const string Monsters = "/api/pb/monsters";

    public const string BattlesProto = "/api/pb/battles";
    public const string ChaptersProto = "/api/pb/chapters";
    public const string StagesProto = "/api/pb/stages";
    public static string UserInventoryList(int userId) =>
        $"/api/pb/users/{userId}/inventory";

    public static string UserCharacterList(int userId) =>
        $"/api/pb/userCharacters/{userId}";

    public static string UserCharacterEquip(int userId, int characterId, int equipId)
        => $"/api/pb/users/{userId}/characters/{characterId}/equipment/{equipId}";

    public static string UserPartyGet(int useId, int partyId) => $"/api/pb/userparty/by-battle?userId={useId}&battleId={partyId}";
    public const string UserPartyBulkAssign = "/api/pb/userparty/bulk-assign";

    #region Combat

    public const string CombatStart = "/api/pb/combat/start";
    public static string CombatCommand(long combatId) => $"/api/pb/combat/{combatId}/command";
    public static string CombatSummary(long combatId) => $"/api/pb/combat/{combatId}/summary";
    public static string CombatLog(long combatId, string cursor, int size) => $"/api/pb/combat/{combatId}/log?cursor={cursor}&size={size}";
    public static string CombatTick(long combatId) => $"/api/pb/combat/{combatId}/tick";
    public static string CombatFinish(long combatId) => $"/api/pb/combat/{combatId}/finish";
    #endregion

    public const string Ping = "/api/ping";
}
