
using Contracts.Protos;
using Contracts.UserParty;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserData
{
    // Base info
    public int UserId { get; private set; }
    public string Nickname { get; private set; }
    public int Level { get; private set; }

    // Currency
    public int SoftCurrency { get; private set; }
    public int HardCurrency { get; private set; }

    public UserProfilePb UserProfilePb { get; private set; }

    // Inventory
    private readonly Dictionary<long, UserInventory> _inventory = new(); // ItemId -> UserInventory
    public Dictionary<long, UserInventory> Inventory => _inventory;

    private readonly Dictionary<int, List<UserInventory>> _inventoryType = new(); // Inventory IDs grouped by item type
    public IReadOnlyDictionary<int, List<UserInventory>> InventoryType => _inventoryType;

    // Owned characters
    private readonly Dictionary<int, UserCharacterSummaryPb> _userCharactersDict = new();

    public Dictionary<int, UserCharacterSummaryPb> UserCharactersDict
        => _userCharactersDict.ToDictionary(kv => kv.Key, kv => kv.Value.Clone()); // Clone to prevent external mutation

    public StageProgressManager StageProgress { get; } = new StageProgressManager();

    // Party
    private readonly Dictionary<int, List<UserPartySlotPb>> _userpartyList = new(); // Party info per BattleType
    public IReadOnlyDictionary<int, List<UserPartySlotPb>> UserPartyList => _userpartyList;
    private readonly Dictionary<int, long> _userPartyIdByBattleId = new(); // Party ID by BattleType
    public IReadOnlyDictionary<int, long> UserPartyIdByBattleId => _userPartyIdByBattleId;
    public GetUserPartyResponsePb PartyProgress { get; } = new GetUserPartyResponsePb();


    public UserData(int userId, string nickname, int level)
    {
        UserId = userId;
        Nickname = nickname;
        Level = level;
    }
    public void SyncCurrencies(int soft, int hard)
    {
        SoftCurrency = soft;
        HardCurrency = hard;
    }
    public void SyncInventory(IEnumerable<UserInventory> items)
    {
        _inventory.Clear();
        _inventoryType.Clear();

        foreach (var i in items)
        {
            _inventory[i.Id] = i;

            int TypeNum = ItemCache.Instance.ItemDict[i.ItemId].TypeId;

            if (!_inventoryType.ContainsKey(TypeNum))
                _inventoryType[TypeNum] = new List<UserInventory>();
            _inventoryType[TypeNum].Add(i);
            Debug.Log($"Inventory sync - category {TypeNum} : itemId {i.ItemId} : total count {_inventory.Count}");
        }

    }
    public int GetItemCount(int itemId)
            => _inventory.TryGetValue(itemId, out var cnt) ? cnt.Count : 0;

    public void SyncCharacters(IEnumerable<UserCharacterSummaryPb> userCharacters)
    {
        var incoming = new Dictionary<int, UserCharacterSummaryPb>();

        Debug.Log($"1 [UserCharacters] {userCharacters.Count()}");

        foreach (var uc in userCharacters)
            incoming[uc.CharacterId] = uc;

        // Add/Update: only apply if UpdatedAt is more recent
        foreach (var (charId, inc) in incoming)
        {
            if (_userCharactersDict.TryGetValue(charId, out var cur))
            {
                if (ToDto(inc.UpdatedAt) > ToDto(cur.UpdatedAt))
                    _userCharactersDict[charId] = inc.Clone();
            }
            else
            {
                _userCharactersDict[charId] = inc.Clone();
            }
        }

        Debug.Log($"2 [UserCharacters] {incoming.Count()}");
    }
    public void ApplyEquipmentSnapshot(SetEquipmentResponse res)
    {
        if (!UserCharactersDict.TryGetValue(res.CharacterId, out var ch))
        {
            Debug.LogError($"Character {res.CharacterId} not found in UserData");
            return;
        }

        // Clone has no setter, so replace with new instance
        var updated = ch.Clone();

        // Clear equips and apply updated values
        updated.Equips.Clear();
        updated.Equips.AddRange(res.Equips);

        // Write back to dict (property returns Clone so must set via dict directly)
        _userCharactersDict[res.CharacterId] = updated;
    }

    public bool AddOrUpdateCharacter(UserCharacterSummaryPb character)
    {
        var inc = character;
        if (_userCharactersDict.TryGetValue(inc.CharacterId, out var cur))
        {
            if (ToDto(inc.UpdatedAt) <= ToDto(cur.UpdatedAt))
                return false; // Outdated, skip
        }
        _userCharactersDict[inc.CharacterId] = inc.Clone();
        return true;
    }

   public void SyncUserParty(int battleId, long partyid, IEnumerable<UserPartySlotPb> userPartys)
    {
        _userPartyIdByBattleId[battleId] = partyid;
        _userpartyList[battleId] = new List<UserPartySlotPb>();
        foreach (var userPartySlot in userPartys)
        {
            _userpartyList[battleId].Add(userPartySlot);
        }
    }

    public bool RemoveCharacter(int characterId) => _userCharactersDict.Remove(characterId);

    // Skill upsert (skill cache can be added later if needed)
    public bool UpsertSkill(int characterId, UserCharacterSkillPb skill)
    {
        if (!_userCharactersDict.TryGetValue(characterId, out var ch)) return false;

        var list = ch.Skills;
        var idx = list.ToList().FindIndex(s => s.SkillId == skill.SkillId);

        if (idx >= 0)
        {
            // Apply most recent data
            var cur = list[idx];
            if (ToDto(skill.UpdatedAt) <= ToDto(cur.UpdatedAt)) return false;

            list[idx] = skill.Clone();
        }
        else
        {
            list.Add(skill.Clone());
        }

        // Update character's UpdatedAt (consistent with policy)
        ch.UpdatedAt = skill.UpdatedAt;
        return true;
    }

    public List<UserCharacterSummaryPb> GetAllUserCharacters()
    => _userCharactersDict.Values
                         .Select(x => x.Clone())
                         .ToList();
    public UserCharacterSummaryPb TryGetCharacter(int characterId)
        => _userCharactersDict.TryGetValue(characterId, out var ch) ? ch.Clone() : null;

    public void SetUserProfile(UserProfilePb _userProfile)
    {
        UserProfilePb = _userProfile;
        Debug.Log($"[SetUserProfile] {UserProfilePb == null}");
    }
    private static DateTimeOffset ToDto(Timestamp ts)
        => ts == null ? DateTimeOffset.MinValue : ts.ToDateTimeOffset();

    public void SyncStageProgress(MyStageProgressListPb pb)
    {
        StageProgress.Sync(pb);
    }
    public bool TryGetStageProgress(int stageId, out UserStageProgressPb progress)
    {
        var p = StageProgress.GetProgress(stageId);
        progress = p;
        return p != null;
    }

    public int GetStars(int stageId)
    {
        var p = StageProgress.GetProgress(stageId);
        return p == null ? 0 : (int)p.Stars;
    }

    public bool IsStageCleared(int stageId)
    {
        var p = StageProgress.GetProgress(stageId);
        return p != null && p.Cleared;
    }
}
