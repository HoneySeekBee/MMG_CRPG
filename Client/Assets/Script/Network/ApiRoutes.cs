using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ApiRoutes
{
    public const string AuthRegister = "/auth/register";
    public const string Status = "/status";
    public const string AuthLogin = "/auth/login";
    public const string AuthGuest = "/auth/guest"; // 예시: 게스트 로그인
    public const string AuthRefresh = "/auth/refresh";
    public const string PlayerBootstrap = "/player/bootstrap";

    public const string MeSummary = "/me/summary";
    public const string MeProfile = "/me/profile";
    public const string UserStageProgress = "/me/stages";
    public const string MeProfileUpdate = "/me/profile";
    public const string MeChangePassword = "/me/change-password"; 

    public const string ItemTypes = "/itemtypes";
    public const string EquipSlots = "/equipslots";
    public const string MasterData = "/masterdata";
    public const string Portraits = "/portraits";
    public const string Icons = "/icons";
    public const string Items = "/items";

    public const string Skills = "/skills";
    public const string CharacterExp = "/character-exp";
    public const string Character = "/characters";
    public const string CharacterModel_List = "/character-model/list";
    public const string CharacterModel_Parts = "/character-model/parts";
    public const string CharacterModel_Weapons = "/character-model/weapons";


    public const string Monsters = "/monsters";

    public const string BattlesProto = "/battles";
    public const string ChaptersProto = "/chapters";
    public const string StagesProto = "/stages";
    public static string UserInventoryList(int userId) =>
        $"/users/{userId}/inventory";

    public static string UserCharacterList(int userId) =>
        $"/userCharacters/{userId}";

    public static string UserCharacterEquip(int userId, int characterId, int equipId)
        => $"/users/{userId}/characters/{characterId}/equipment/{equipId}";

    public static string UserPartyGet(int useId, int partyId) => $"/userparty/by-battle?userId={useId}&battleId={partyId}";
    public const string UserPartyBulkAssign = "/userparty/bulk-assign";

    #region Combat

    public const string CombatStart =  "/combat/start";
    public static string CombatCommand(long combatId) => $"/combat/{combatId}/command";
    public static string CombatSummary(long combatId) => $"/combat/{combatId}/summary";
    public static string CombatLog(long combatId, string cursor, int size) => $"/combat/{combatId}/log?cursor={cursor}&size={size}";
    #endregion

}
