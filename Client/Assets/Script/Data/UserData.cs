
using Contracts.Protos;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserData
{
    // 기본 정보
    public int UserId { get; private set; }
    public string Nickname { get; private set; }
    public int Level { get; private set; }

    // 재화
    public int SoftCurrency { get; private set; }
    public int HardCurrency { get; private set; }

    public UserProfilePb UserProfilePb { get; private set; }

    // 인벤토리
    private readonly Dictionary<int, int> _inventory = new(); // ItemId -> Count
    public IReadOnlyDictionary<int, int> Inventory => _inventory;

    private readonly Dictionary<int, List<int>> _inventoryType = new(); // 각 타입 별 인벤토리 Id 
    public IReadOnlyDictionary<int, List<int>> InventoryType => _inventoryType;

    // 보유 캐릭터 
    private readonly Dictionary<int, UserCharacterSummaryPb> _userCharactersDict = new();

    public IReadOnlyDictionary<int, UserCharacterSummaryPb> UserCharactersDict
        => _userCharactersDict.ToDictionary(kv => kv.Key, kv => kv.Value.Clone()); // 외부에 Clone


    // 스테이지 진행
    private readonly HashSet<int> _clearedStages = new();
    public IReadOnlyCollection<int> ClearedStages => _clearedStages;

   
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
            _inventory[i.ItemId] = i.Count;
            
            int TypeNum = ItemCache.Instance.ItemDict[i.ItemId].TypeId;

            if (!_inventoryType.ContainsKey(TypeNum))
                _inventoryType[TypeNum] = new List<int>();
            _inventoryType[TypeNum].Add(i.ItemId);
            Debug.Log($"유저의 인벤토리 아이템 카테고리 {TypeNum} : 아이디 {i.ItemId} : 갯수 {_inventory.Count}");
        }

    }
    public void UpdateInventoryItem(int itemId, int count)
    {
        if (count <= 0) _inventory.Remove(itemId);
        else _inventory[itemId] = count;
    }
    public int GetItemCount(int itemId)
            => _inventory.TryGetValue(itemId, out var cnt) ? cnt : 0;

    public void SyncCharacters(IEnumerable<UserCharacterSummaryPb> userCharacters)
    { 
        var incoming = new Dictionary<int, UserCharacterSummaryPb>();

        Debug.Log($"[UserCharacters] {userCharacters.Count()}");

        foreach (var uc in userCharacters)
            incoming[uc.CharacterId] = uc; 
        
        // 2) 제거: 서버에 없는 캐릭터는 로컬에서 제거 
        var toRemove = _userCharactersDict.Keys.Except(incoming.Keys).ToList();
        foreach (var key in toRemove)
            _userCharactersDict.Remove(key);

        // 3) 추가/업데이트: UpdatedAt이 더 최신인 경우만 덮어쓰기
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

    }
     public bool AddOrUpdateCharacter(UserCharacterSummaryPb character)
    {
        var inc = character;
        if (_userCharactersDict.TryGetValue(inc.CharacterId, out var cur))
        {
            if (ToDto(inc.UpdatedAt) <= ToDto(cur.UpdatedAt))
                return false; // 구버전이면 무시
        }
        _userCharactersDict[inc.CharacterId] = inc.Clone();
        return true;
    }

    public bool RemoveCharacter(int characterId) => _userCharactersDict.Remove(characterId);

    // 스킬 업서트 (필요 시 스킬 딕셔너리 캐시 추가 고려)
    public bool UpsertSkill(int characterId, UserCharacterSkillPb skill)
    {
        if (!_userCharactersDict.TryGetValue(characterId, out var ch)) return false;

        var list = ch.Skills;
        var idx = list.ToList().FindIndex(s => s.SkillId == skill.SkillId);

        if (idx >= 0)
        {
            // 최신 것만 반영
            var cur = list[idx];
            if (ToDto(skill.UpdatedAt) <= ToDto(cur.UpdatedAt)) return false;

            list[idx] = skill.Clone();
        }
        else
        {
            list.Add(skill.Clone());
        }

        // 캐릭터 UpdatedAt도 갱신(정책에 맞게)
        ch.UpdatedAt = skill.UpdatedAt;
        return true;
    }

    public List<UserCharacterSummaryPb> GetAllUserCharacters()
    => _userCharactersDict.Values
                         .Select(x => x.Clone())
                         .ToList();
    public UserCharacterSummaryPb TryGetCharacter(int characterId)
        => _userCharactersDict.TryGetValue(characterId, out var ch) ? ch.Clone() : null; 
    
    public void SyncStages(IEnumerable<int> clearedStageIds)
    {
        _clearedStages.Clear();
        foreach (var id in clearedStageIds) _clearedStages.Add(id);
    }
    public void SetUserProfile(UserProfilePb _userProfile)
    {
        UserProfilePb = _userProfile;
        Debug.Log($"[SetUserProfile] {UserProfilePb == null}");
    }
    public void MarkStageCleared(int stageId) => _clearedStages.Add(stageId);

    private static DateTimeOffset ToDto(Timestamp ts)
        => ts == null ? DateTimeOffset.MinValue : ts.ToDateTimeOffset();
}
