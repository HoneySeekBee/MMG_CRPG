
using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
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
    private readonly HashSet<int> _characters = new(); // CharacterId
    public IReadOnlyCollection<int> Characters => _characters;

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

    public void SyncCharacters(IEnumerable<int> characterIds)
    {
        _characters.Clear();
        foreach (var id in characterIds) _characters.Add(id);
    }

    public void AddCharacter(int charId) => _characters.Add(charId);

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
}
